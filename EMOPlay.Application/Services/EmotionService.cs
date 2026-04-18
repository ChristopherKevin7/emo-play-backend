using EMOPlay.Application.DTOs.Emotion;
using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMOPlay.Application.Services;

/// <summary>
/// Serviço para análise de emoções com suporte a multi-frame batch processing
/// </summary>
public class EmotionService : IEmotionService
{
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmotionService> _logger;
    private const double ScoreThreshold = 0.4; // Score mínimo para considerar correto

    public EmotionService(IAIService aiService, IUnitOfWork unitOfWork, ILogger<EmotionService> logger)
    {
        _aiService = aiService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BatchAnalyzeResponse> BatchAnalyzeAsync(BatchAnalyzeRequest request)
    {
        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        _logger.LogInformation("========================================");
        _logger.LogInformation("[EmotionService] BATCH ANALYZE - INÍCIO");
        _logger.LogInformation("[EmotionService] SessionId: {SessionId}", sessionId);
        _logger.LogInformation("[EmotionService] UserId: {UserId}", request.UserId);
        _logger.LogInformation("[EmotionService] Total de emoções recebidas: {Count}", request.Attempts?.Count ?? 0);

        if (request.Attempts != null)
        {
            for (int i = 0; i < request.Attempts.Count; i++)
            {
                var a = request.Attempts[i];
                _logger.LogInformation("[EmotionService] Emoção [{Index}]: target='{Emotion}' | {FrameCount} frames | tamanhos={Sizes}",
                    i + 1, a.TargetEmotion, a.Images?.Count ?? 0,
                    string.Join(", ", a.Images?.Select(img => $"{img?.Length ?? 0}ch") ?? Array.Empty<string>()));
            }
        }

        if (request.Attempts == null || request.Attempts.Count == 0)
            throw new ArgumentException("Pelo menos uma tentativa deve ser fornecida", nameof(request.Attempts));

        var results = new List<BatchAnalyzeResult>();
        var correctCount = 0;
        var processedAt = DateTime.UtcNow;

        // Processar cada emoção (sequencial para logs legíveis, paralelo dentro do AI call)
        foreach (var attempt in request.Attempts)
        {
            try
            {
                var result = await ProcessAttemptAsync(attempt, processedAt);
                results.Add(result);
                if (result.IsCorrect)
                    correctCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError("[EmotionService] ERRO ao processar emoção '{Emotion}': {Message}",
                    attempt.TargetEmotion, ex.Message);
            }
        }

        var total = request.Attempts.Count;
        var accuracy = total > 0 ? (double)correctCount / total : 0;

        var response = new BatchAnalyzeResponse
        {
            SessionId = sessionId,
            Total = total,
            Correct = correctCount,
            Accuracy = accuracy,
            Results = results,
            ProcessedAt = processedAt,
            Message = $"Batch analysis concluído: {correctCount}/{total} corretas ({accuracy:P2})"
        };

        await SaveBatchResultAsync(request, response, sessionId);

        // Log resultado final
        _logger.LogInformation("[EmotionService] ===== RESULTADO FINAL =====");
        _logger.LogInformation("[EmotionService] SessionId: {SessionId}", sessionId);
        _logger.LogInformation("[EmotionService] Total: {Total} | Corretos: {Correct} | Accuracy: {Accuracy:P2}",
            total, correctCount, accuracy);
        foreach (var r in results)
        {
            var topEmotion = r.TopPredictions.FirstOrDefault();
            _logger.LogInformation("[EmotionService] [{Status}] Target='{Target}' | Top1='{Top1}' ({Score1:F4}) | TargetScore={TargetScore:F4}",
                r.IsCorrect ? "✓" : "✗",
                r.TargetEmotion,
                topEmotion?.Emotion ?? "N/A",
                topEmotion?.Score ?? 0,
                r.AverageScores.GetValueOrDefault(r.TargetEmotion.ToLowerInvariant(), 0));
        }
        _logger.LogInformation("========================================");

        return response;
    }

    /// <summary>
    /// Processa uma tentativa: envia N frames para IA, agrega scores e aplica regras
    /// </summary>
    private async Task<BatchAnalyzeResult> ProcessAttemptAsync(BatchAttempt attempt, DateTime processedAt)
    {
        _logger.LogInformation("[EmotionService] --- Processando emoção: '{Emotion}' ({FrameCount} frames) ---",
            attempt.TargetEmotion, attempt.Images.Count);

        // 1. Enviar todas as imagens para o módulo de IA
        var aiResult = await _aiService.AnalyzeBatchAsync(attempt.Images, attempt.TargetEmotion);

        _logger.LogInformation("[EmotionService] Predictions recebidas da IA: {Count}", aiResult.Predictions.Count);
        foreach (var pred in aiResult.Predictions)
        {
            _logger.LogInformation("[EmotionService]   → {Emotion}: {Score:F4}", pred.Emotion, pred.Score);
        }

        // 2. Montar average scores a partir das predictions
        var averageScores = new Dictionary<string, double>();
        foreach (var pred in aiResult.Predictions)
        {
            var normalizedEmotion = pred.Emotion.Trim().ToLowerInvariant();
            averageScores[normalizedEmotion] = pred.Score;
        }

        // 3. Ordenar predictions por score (top predictions)
        var topPredictions = aiResult.Predictions
            .OrderByDescending(p => p.Score)
            .Select(p => new EmotionPrediction
            {
                Emotion = p.Emotion.Trim().ToLowerInvariant(),
                Score = p.Score
            })
            .ToList();

        // 4. Aplicar regra de avaliação
        var targetNormalized = attempt.TargetEmotion.Trim().ToLowerInvariant();
        var isInTop2 = topPredictions.Take(2).Any(p => p.Emotion == targetNormalized);
        var targetScore = averageScores.GetValueOrDefault(targetNormalized, 0);
        var passesScoreThreshold = targetScore > ScoreThreshold;

        var isCorrect = isInTop2 || passesScoreThreshold;

        _logger.LogInformation("[EmotionService] Avaliação para '{Target}':", targetNormalized);
        _logger.LogInformation("[EmotionService]   Top 2: [{Top2}]",
            string.Join(", ", topPredictions.Take(2).Select(p => $"{p.Emotion}:{p.Score:F4}")));
        _logger.LogInformation("[EmotionService]   Target no Top 2: {InTop2}", isInTop2);
        _logger.LogInformation("[EmotionService]   Target score: {Score:F4} | Threshold: {Threshold} | Passa: {Pass}",
            targetScore, ScoreThreshold, passesScoreThreshold);
        _logger.LogInformation("[EmotionService]   → IsCorrect: {IsCorrect} (inTop2={InTop2} OR score>{Threshold}={Pass})",
            isCorrect, isInTop2, ScoreThreshold, passesScoreThreshold);

        return new BatchAnalyzeResult
        {
            TargetEmotion = attempt.TargetEmotion,
            TopPredictions = topPredictions,
            AverageScores = averageScores,
            IsCorrect = isCorrect,
            AnalysisTimestamp = processedAt
        };
    }

    /// <summary>
    /// Salva os resultados do batch no MongoDB
    /// </summary>
    private async Task SaveBatchResultAsync(BatchAnalyzeRequest request, BatchAnalyzeResponse response, string sessionId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(request.UserId));
            if (user == null)
            {
                _logger.LogWarning("[EmotionService] Usuário {UserId} não encontrado, salvamento ignorado", request.UserId);
                return;
            }

            var sessionResult = new EMOPlay.Domain.Entities.SessionResult
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.Parse(sessionId),
                CorrectAnswers = response.Correct,
                TotalChallenges = response.Total,
                Percentage = response.Accuracy * 100,
                Message = response.Message,
                ResultsJson = System.Text.Json.JsonSerializer.Serialize(response.Results),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SessionResults.AddAsync(sessionResult);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[EmotionService] Resultados salvos no MongoDB para sessão {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError("[EmotionService] Erro ao salvar resultados: {Message}", ex.Message);
        }
    }
}
