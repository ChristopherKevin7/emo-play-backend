using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMOPlay.Application.Services;

public class ScoreService : IScoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScoreService> _logger;

    // Pontos por acerto por nível — Desafio 1
    private static readonly Dictionary<string, int> Desafio1PointsPerHit = new(StringComparer.OrdinalIgnoreCase)
    {
        { "easy",   10 },
        { "medium", 20 },
        { "hard",   30 }
    };

    // Pontos por acerto fixo — Desafio 2
    private const int Desafio2PointsPerHit = 50;

    public ScoreService(IUnitOfWork unitOfWork, ILogger<ScoreService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public int CalculateDesafio1Score(int correctCount, string level)
    {
        if (!Desafio1PointsPerHit.TryGetValue(level, out var pointsPerHit))
        {
            _logger.LogWarning("[ScoreService] Nível desconhecido '{Level}', usando 'easy' (10 pts)", level);
            pointsPerHit = 10;
        }

        var score = correctCount * pointsPerHit;
        _logger.LogInformation("[ScoreService] Desafio 1 | {Correct} acertos × {Pts} pts ({Level}) = {Score} pts",
            correctCount, pointsPerHit, level, score);
        return score;
    }

    public int CalculateDesafio2Score(int correctCount)
    {
        var score = correctCount * Desafio2PointsPerHit;
        _logger.LogInformation("[ScoreService] Desafio 2 | {Correct} acertos × {Pts} pts = {Score} pts",
            correctCount, Desafio2PointsPerHit, score);
        return score;
    }

    public async Task AddUserPointsAsync(Guid userId, int points)
    {
        if (points <= 0) return;

        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("[ScoreService] Usuário {UserId} não encontrado, pontos não adicionados", userId);
                return;
            }

            user.Points += points;
            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[ScoreService] Usuário {UserId} | +{Points} pts → total: {Total} pts",
                userId, points, user.Points);
        }
        catch (Exception ex)
        {
            _logger.LogError("[ScoreService] Erro ao adicionar pontos ao usuário {UserId}: {Message}", userId, ex.Message);
            throw;
        }
    }

    public async Task<int> GetUserPointsAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"Usuário {userId} não encontrado");

        return user.Points;
    }
}
