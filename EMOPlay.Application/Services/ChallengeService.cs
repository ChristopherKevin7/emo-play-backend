using EMOPlay.Application.DTOs.Challenge;
using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Entities;
using EMOPlay.Domain.Enums;
using EMOPlay.Domain.Interfaces;

namespace EMOPlay.Application.Services;

public class ChallengeService : IChallengeService
{
    private readonly IUnitOfWork _unitOfWork;
    private const int CorrectAnswerPoints = 10;

    public ChallengeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RecordResponseResponse> RecordResponseAsync(RecordResponseRequest request)
    {
        var gameSession = await _unitOfWork.GameSessions.GetByIdAsync(request.SessionId);
        
        if (gameSession == null)
            throw new InvalidOperationException("Game session not found");

        var targetEmotionEnum = Enum.Parse<EmotionEnum>(request.TargetEmotion, ignoreCase: true);
        var childResponseEnum = Enum.Parse<EmotionEnum>(request.ChildResponse, ignoreCase: true);

        var challenge = new Challenge
        {
            Id = request.ChallengeId,
            SessionId = request.SessionId,
            TargetEmotion = targetEmotionEnum,
            ChildResponse = childResponseEnum,
            IsCorrect = request.IsCorrect,
            ResponseTimeMs = request.ResponseTimeMs,
            Confidence = request.Confidence,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Challenges.AddAsync(challenge);

        var points = request.IsCorrect ? CorrectAnswerPoints : 0;
        gameSession.TotalPoints += points;

        _unitOfWork.GameSessions.Update(gameSession);
        await _unitOfWork.SaveChangesAsync();

        return new RecordResponseResponse
        {
            Recorded = true,
            Points = points,
            TotalPoints = gameSession.TotalPoints,
            Message = "Response recorded successfully"
        };
    }
}
