using EMOPlay.Application.DTOs.Metrics;
using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Interfaces;
using EMOPlay.Domain.Enums;

namespace EMOPlay.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly IUnitOfWork _unitOfWork;
    private const string Disclaimer = "These metrics are for educational purposes and do not replace professional evaluation.";

    public MetricsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChildMetricsResponse> GetChildMetricsAsync(Guid childId, Guid? sessionId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Busca o usuário (criança) com role = Child
        var child = await _unitOfWork.Users.GetByIdAsync(childId);
        
        if (child == null || child.Role != RoleEnum.Child)
            throw new InvalidOperationException("Child not found");

        // Busca todas as sessões de jogo dessa criança
        var sessions = await _unitOfWork.GameSessions.FindAsync(s => s.ChildId == childId);
        
        // Aplicar filtros opcionais
        if (sessionId.HasValue)
            sessions = sessions.Where(s => s.Id == sessionId.Value).ToList();

        if (startDate.HasValue)
            sessions = sessions.Where(s => s.StartedAt.Date >= startDate.Value.Date).ToList();

        if (endDate.HasValue)
            sessions = sessions.Where(s => s.StartedAt.Date <= endDate.Value.Date).ToList();

        // Busca todos os desafios de todas as sessões
        var allChallenges = new List<Domain.Entities.Challenge>();
        foreach (var session in sessions)
        {
            var challenges = await _unitOfWork.Challenges.FindAsync(c => c.SessionId == session.Id);
            allChallenges.AddRange(challenges);
        }

        // Monta dados para Desafio_1 (Identificação de Emoções)
        var desafio_1 = BuildDesafio1Metrics(allChallenges, sessions);

        // Retorna com Desafio_2 como null (será preenchido no futuro)
        return new ChildMetricsResponse
        {
            ChildId = childId,
            ChildName = child.Name,
            Desafio_1 = desafio_1,
            Desafio_2 = null, // Futuro
            Disclaimer = Disclaimer
        };
    }

    private Desafio1MetricsData BuildDesafio1Metrics(List<Domain.Entities.Challenge> allChallenges, List<Domain.Entities.GameSession> sessions)
    {
        // Se não houver desafios, retorna estrutura vazia
        if (!allChallenges.Any())
        {
            return new Desafio1MetricsData
            {
                TotalSessions = sessions.Count,
                AccuracyRate = 0,
                AverageResponseTimeMs = 0,
                EmotionBreakdown = new Dictionary<string, Desafio1EmotionBreakdown>(),
                ProgressTrend = new List<ProgressTrendItem>(),
                HistoricAttempts = new List<Desafio1HistoricAttempt>()
            };
        }

        // Calcula métricas consolidadas
        var totalChallenges = allChallenges.Count;
        var correctChallenges = allChallenges.Count(c => c.IsCorrect);
        var accuracyRate = (double)correctChallenges / totalChallenges;

        var emotionBreakdown = BuildDesafio1EmotionBreakdown(allChallenges);
        var progressTrend = BuildProgressTrend(sessions, allChallenges);
        var historicAttempts = BuildDesafio1HistoricAttempts(sessions);

        return new Desafio1MetricsData
        {
            TotalSessions = sessions.Count,
            AccuracyRate = accuracyRate,
            AverageResponseTimeMs = (int)allChallenges.Average(c => c.ResponseTimeMs),
            EmotionBreakdown = emotionBreakdown,
            ProgressTrend = progressTrend,
            HistoricAttempts = historicAttempts
        };
    }

    private Dictionary<string, Desafio1EmotionBreakdown> BuildDesafio1EmotionBreakdown(List<Domain.Entities.Challenge> challenges)
    {
        var breakdown = new Dictionary<string, Desafio1EmotionBreakdown>();

        var emotions = Enum.GetValues(typeof(EmotionEnum)).Cast<EmotionEnum>();

        foreach (var emotion in emotions)
        {
            var emotionChallenges = challenges.Where(c => c.TargetEmotion == emotion).ToList();
            
            if (emotionChallenges.Any())
            {
                var correctAttempts = emotionChallenges.Count(c => c.IsCorrect);
                var accuracy = (double)correctAttempts / emotionChallenges.Count;

                breakdown[emotion.ToString()] = new Desafio1EmotionBreakdown
                {
                    Attempts = emotionChallenges.Count,
                    CorrectAttempts = correctAttempts,
                    Accuracy = accuracy
                };
            }
        }

        return breakdown;
    }

    private Dictionary<string, Desafio2EmotionBreakdown> BuildDesafio2EmotionBreakdown(List<Domain.Entities.Challenge> challenges)
    {
        var breakdown = new Dictionary<string, Desafio2EmotionBreakdown>();

        var emotions = Enum.GetValues(typeof(EmotionEnum)).Cast<EmotionEnum>();

        foreach (var emotion in emotions)
        {
            var emotionChallenges = challenges.Where(c => c.TargetEmotion == emotion).ToList();
            
            if (emotionChallenges.Any())
            {
                var correctAttempts = emotionChallenges.Count(c => c.IsCorrect);
                var accuracy = (double)correctAttempts / emotionChallenges.Count;

                breakdown[emotion.ToString()] = new Desafio2EmotionBreakdown
                {
                    Attempts = emotionChallenges.Count,
                    CorrectAttempts = correctAttempts,
                    Accuracy = accuracy,
                    AvgConfidence = emotionChallenges.Average(c => c.Confidence)
                };
            }
        }

        return breakdown;
    }

    private List<ProgressTrendItem> BuildProgressTrend(List<Domain.Entities.GameSession> sessions, List<Domain.Entities.Challenge> allChallenges)
    {
        var trend = new List<ProgressTrendItem>();

        var sessionsByDate = sessions.GroupBy(s => s.StartedAt.Date).OrderBy(g => g.Key);

        foreach (var dateGroup in sessionsByDate)
        {
            var sessionIds = dateGroup.Select(s => s.Id).ToList();
            var dayChallenges = allChallenges.Where(c => sessionIds.Contains(c.SessionId)).ToList();

            if (dayChallenges.Any())
            {
                var dayAccuracy = (double)dayChallenges.Count(c => c.IsCorrect) / dayChallenges.Count;
                trend.Add(new ProgressTrendItem
                {
                    Date = dateGroup.Key,
                    Accuracy = dayAccuracy
                });
            }
        }

        return trend;
    }

    private List<Desafio1HistoricAttempt> BuildDesafio1HistoricAttempts(List<Domain.Entities.GameSession> sessions)
    {
        var historic = new List<Desafio1HistoricAttempt>();

        // Ordena sessões por data (mais recente primeiro)
        var sortedSessions = sessions.OrderByDescending(s => s.StartedAt).ToList();

        foreach (var session in sortedSessions)
        {
            var attempt = new Desafio1HistoricAttempt
            {
                Date = session.StartedAt,
                AccuracyRate = session.AccuracyRate ?? 0,
                Level = session.Level
            };
            historic.Add(attempt);
        }

        return historic;
    }
}
