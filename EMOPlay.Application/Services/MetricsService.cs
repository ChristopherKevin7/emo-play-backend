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
        var child = await _unitOfWork.Children.GetByIdAsync(childId);
        
        if (child == null)
            throw new InvalidOperationException("Child not found");

        var sessions = await _unitOfWork.GameSessions.FindAsync(s => s.ChildId == childId);
        
        if (sessionId.HasValue)
            sessions = sessions.Where(s => s.Id == sessionId.Value).ToList();

        if (startDate.HasValue)
            sessions = sessions.Where(s => s.StartedAt.Date >= startDate.Value.Date).ToList();

        if (endDate.HasValue)
            sessions = sessions.Where(s => s.StartedAt.Date <= endDate.Value.Date).ToList();

        if (!sessions.Any())
            return new ChildMetricsResponse
            {
                ChildId = childId,
                ChildName = child.Name,
                TotalSessions = 0,
                AccuracyRate = 0,
                AverageResponseTimeMs = 0,
                AverageConfidence = 0,
                EmotionBreakdown = new Dictionary<string, EmotionMetricsBreakdown>(),
                ProgressTrend = new List<ProgressTrendItem>(),
                Disclaimer = Disclaimer
            };

        var allChallenges = new List<Domain.Entities.Challenge>();
        foreach (var session in sessions)
        {
            var challenges = await _unitOfWork.Challenges.FindAsync(c => c.SessionId == session.Id);
            allChallenges.AddRange(challenges);
        }

        if (!allChallenges.Any())
            return new ChildMetricsResponse
            {
                ChildId = childId,
                ChildName = child.Name,
                TotalSessions = sessions.Count,
                AccuracyRate = 0,
                AverageResponseTimeMs = 0,
                AverageConfidence = 0,
                EmotionBreakdown = new Dictionary<string, EmotionMetricsBreakdown>(),
                ProgressTrend = new List<ProgressTrendItem>(),
                Disclaimer = Disclaimer
            };

        var totalChallenges = allChallenges.Count;
        var correctChallenges = allChallenges.Count(c => c.IsCorrect);
        var accuracyRate = (double)correctChallenges / totalChallenges;

        var emotionBreakdown = BuildEmotionBreakdown(allChallenges);
        var progressTrend = BuildProgressTrend(sessions, allChallenges);

        return new ChildMetricsResponse
        {
            ChildId = childId,
            ChildName = child.Name,
            TotalSessions = sessions.Count,
            AccuracyRate = accuracyRate,
            AverageResponseTimeMs = (int)allChallenges.Average(c => c.ResponseTimeMs),
            AverageConfidence = allChallenges.Average(c => c.Confidence),
            EmotionBreakdown = emotionBreakdown,
            ProgressTrend = progressTrend,
            Disclaimer = Disclaimer
        };
    }

    private Dictionary<string, EmotionMetricsBreakdown> BuildEmotionBreakdown(List<Domain.Entities.Challenge> challenges)
    {
        var breakdown = new Dictionary<string, EmotionMetricsBreakdown>();

        var emotions = Enum.GetValues(typeof(EmotionEnum)).Cast<EmotionEnum>();

        foreach (var emotion in emotions)
        {
            var emotionChallenges = challenges.Where(c => c.TargetEmotion == emotion).ToList();
            
            if (emotionChallenges.Any())
            {
                var correctAttempts = emotionChallenges.Count(c => c.IsCorrect);
                var accuracy = (double)correctAttempts / emotionChallenges.Count;

                breakdown[emotion.ToString()] = new EmotionMetricsBreakdown
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
}
