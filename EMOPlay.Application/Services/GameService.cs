using EMOPlay.Application.DTOs.Game;
using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Entities;
using EMOPlay.Domain.Enums;
using EMOPlay.Domain.Interfaces;
using System.Text.Json;

namespace EMOPlay.Application.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork _unitOfWork;

    public GameService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionResultResponse> SaveGameResultAsync(SaveGameResultRequest request)
    {
        // Validate child exists
        var child = await _unitOfWork.Users.GetByIdAsync(request.ChildId);
        if (child == null)
            throw new InvalidOperationException($"Child with ID {request.ChildId} not found");

        // Parse game mode
        var gameMode = request.GameMode.ToLower() == "identify" 
            ? GameModeEnum.IdentifyEmotion 
            : GameModeEnum.MakeEmotion;

        // Create game session
        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            ChildId = request.ChildId,
            GameMode = gameMode,
            Status = GameSessionStatusEnum.Completed,
            TotalPoints = request.Statistics.CorrectAnswers * 10, // 10 pontos por acerto
            StartedAt = request.StartTime,
            EndedAt = request.EndTime,
            AccuracyRate = request.Statistics.AccuracyRate
        };

        await _unitOfWork.GameSessions.AddAsync(gameSession);

        // Create challenges for each result
        var challenges = new List<Challenge>();
        foreach (var result in request.Results)
        {
            // Parse emotions from string to enum
            var targetEmotionEnum = ParseEmotion(result.TargetEmotion);
            var detectedEmotionEnum = ParseEmotion(result.DetectedEmotion);

            var challenge = new Challenge
            {
                Id = Guid.NewGuid(),
                SessionId = gameSession.Id,
                TargetEmotion = targetEmotionEnum,
                ChildResponse = detectedEmotionEnum,
                IsCorrect = result.IsCorrect,
                ResponseTimeMs = (int)result.ResponseTime,
                Confidence = result.IsCorrect ? 1.0 : 0.0, // Simplificado, pode receber do frontend se disponível
                ImageUrl = "",  // Pode ser adicionado ao request se necessário
                CreatedAt = result.Timestamp
            };

            challenges.Add(challenge);
        }

        foreach (var challenge in challenges)
        {
            await _unitOfWork.Challenges.AddAsync(challenge);
        }

        // Create session result with consolidated statistics
        var challengeResultsJson = JsonSerializer.Serialize(request.Results);

        var sessionResult = new SessionResult
        {
            Id = Guid.NewGuid(),
            SessionId = gameSession.Id,
            CorrectAnswers = request.Statistics.CorrectAnswers,
            TotalChallenges = request.Statistics.TotalChallenges,
            Percentage = request.Statistics.AccuracyRate,
            Message = $"Game completed. Accuracy: {request.Statistics.AccuracyRate:F2}%. Level: {request.LevelId}",
            ResultsJson = challengeResultsJson,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SessionResults.AddAsync(sessionResult);
        await _unitOfWork.SaveChangesAsync();

        // Return response
        return new SessionResultResponse
        {
            SessionId = gameSession.Id,
            Acertos = request.Statistics.CorrectAnswers,
            Percentage = request.Statistics.AccuracyRate,
            Mensagem = $"Jogo finalizado com sucesso! Acertos: {request.Statistics.CorrectAnswers}/{request.Statistics.TotalChallenges}",
            Resultados = new List<ResultadoItem>(), // Pode ser preenchido se necessário
            Armazenado = true,
            Mensagem_Retorno = "Resultados armazenados com sucesso!"
        };
    }

    public async Task<SessionResultResponse> GetSessionResultAsync(Guid sessionId)
    {
        var sessionResult = await _unitOfWork.SessionResults.FirstOrDefaultAsync(sr => sr.SessionId == sessionId);
        
        if (sessionResult == null)
            throw new InvalidOperationException("Session result not found");

        var resultados = JsonSerializer.Deserialize<List<ResultadoItem>>(sessionResult.ResultsJson) ?? new List<ResultadoItem>();

        return new SessionResultResponse
        {
            SessionId = sessionResult.SessionId,
            Acertos = sessionResult.CorrectAnswers,
            Percentage = sessionResult.Percentage,
            Mensagem = sessionResult.Message,
            Resultados = resultados,
            Armazenado = true,
            Mensagem_Retorno = "Resultados recuperados com sucesso!"
        };
    }

    private EmotionEnum ParseEmotion(string emotionName)
    {
        // Convert string emotion to enum
        return emotionName.ToLower() switch
        {
            "happiness" or "happy" => EmotionEnum.Happy,
            "sadness" or "sad" => EmotionEnum.Sad,
            "anger" or "angry" => EmotionEnum.Angry,
            "surprise" or "surprised" => EmotionEnum.Surprised,
            "fear" or "fearful" => EmotionEnum.Fearful,
            "disgust" or "disgusted" => EmotionEnum.Disgusted,
            "neutral" => EmotionEnum.Neutral,
            _ => throw new InvalidOperationException($"Unknown emotion: {emotionName}")
        };
    }
}
