using EMOPlay.Application.DTOs.Emotion;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmotionsController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IEmotionService _emotionService;
    private readonly ILogger<EmotionsController> _logger;

    public EmotionsController(IAIService aiService, IEmotionService emotionService, ILogger<EmotionsController> logger)
    {
        _aiService = aiService;
        _emotionService = emotionService;
        _logger = logger;
    }

    /// <summary>
    /// Analisa a emoção detectada em uma imagem capturada
    /// </summary>
    /// <remarks>
    /// Processa uma imagem capturada da webcam e utiliza o módulo de IA (Python)
    /// para detectar a emoção facial e compará-la com a emoção alvo do desafio.
    /// 
    /// **Fluxo de análise:**
    /// 1. Recebe a imagem em base64 e a emoção alvo esperada
    /// 2. Valida se a imagem foi fornecida
    /// 3. Envia para o módulo de IA (Flask/Python na porta 5000)
    /// 4. Módulo detecta a emoção na imagem com score de confiança
    /// 5. Compara emoção detectada com a emoção alvo
    /// 6. Retorna resultado com confiança e se está correto
    /// 
    /// **Emoções supportadas:**
    /// - happy (feliz)
    /// - sad (triste)
    /// - angry (raiva)
    /// - surprised (surpreso)
    /// - fearful (medo)
    /// - disgusted (desgostado)
    /// - neutral (neutro)
    /// 
    /// **Retorna:**
    /// - detectedEmotion: Emoção detectada pela IA
    /// - confidence: Confiança da detecção (0-1)
    /// - isCorrect: Se foi a emoção correta (booleano)
    /// - analysisTimestamp: Momento da análise
    /// </remarks>
    /// <param name="request">Imagem em base64 e emoção alvo a verificar</param>
    /// <returns>Resultado da análise de emoção</returns>
    /// <response code="200">Análise realizada com sucesso</response>
    /// <response code="400">Imagem ou emoção alvo não fornecidas</response>
    /// <response code="500">Erro na análise de IA ou servidor</response>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalyzeEmotionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnalyzeEmotionResponse>> AnalyzeEmotion([FromBody] AnalyzeEmotionRequest request)
    {
        try
        {
            _logger.LogInformation($"Analyzing emotion for session {request.SessionId}");

            // Validate image
            if (string.IsNullOrEmpty(request.Image))
                return BadRequest(new { error = "Image is required" });

            if (string.IsNullOrEmpty(request.TargetEmotion))
                return BadRequest(new { error = "Target emotion is required" });

            // Call AI service (send single image as a list)
            var aiResult = await _aiService.AnalyzeBatchAsync(new List<string> { request.Image }, request.TargetEmotion);

            var topPrediction = aiResult.Predictions.OrderByDescending(p => p.Score).FirstOrDefault();
            var detectedEmotion = topPrediction?.Emotion ?? "unknown";
            var confidence = topPrediction?.Score ?? 0;
            var isCorrect = detectedEmotion.Equals(request.TargetEmotion, StringComparison.OrdinalIgnoreCase);

            var response = new AnalyzeEmotionResponse
            {
                DetectedEmotion = detectedEmotion,
                Confidence = confidence,
                IsCorrect = isCorrect,
                AnalysisTimestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error analyzing emotion: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Processa múltiplas emoções com múltiplos frames por emoção para análise probabilística
    /// </summary>
    /// <remarks>
    /// Permite que o frontend envie múltiplas tentativas com múltiplos frames cada,
    /// processando todas e retornando um resultado consolidado com top predictions.
    /// 
    /// **Fluxo de processamento:**
    /// 1. Recebe um array de tentativas, cada uma com emoção alvo e array de imagens (frames)
    /// 2. Para cada tentativa, envia as imagens para o módulo de IA
    /// 3. IA retorna top 3 predictions com scores por emoção
    /// 4. Aplica regras de avaliação para determinar acerto
    /// 5. Retorna resultado consolidado com averageScores e topPredictions
    /// 6. Salva resultados no MongoDB
    /// 
    /// **Regra de acerto:**
    /// - Emoção alvo está no TOP 2 predictions
    /// - OU score médio da emoção alvo > 0.4
    /// 
    /// **Exemplo de entrada:**
    /// ```json
    /// {
    ///   "userId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "attempts": [
    ///     {
    ///       "targetEmotion": "happy",
    ///       "images": ["base64_frame1...", "base64_frame2...", "base64_frame3..."]
    ///     },
    ///     {
    ///       "targetEmotion": "sad",
    ///       "images": ["base64_frame1...", "base64_frame2...", "base64_frame3..."]
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Requisição com userId, sessionId e array de tentativas</param>
    /// <returns>Resposta consolidada com resultados individuais e estatísticas</returns>
    /// <response code="200">Batch processado com sucesso</response>
    /// <response code="400">Erro na validação de entrada</response>
    /// <response code="500">Erro ao processar batch ou comunicar com IA</response>
    [HttpPost("batch-analyze")]
    [ProducesResponseType(typeof(BatchAnalyzeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchAnalyzeResponse>> BatchAnalyze([FromBody] BatchAnalyzeRequest request)
    {
        try
        {
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            _logger.LogInformation("Iniciando batch analysis para sessão {SessionId} com {Count} emoções",
                sessionId, request.Attempts?.Count ?? 0);

            if (request == null)
                return BadRequest(new { error = "Request is required" });

            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { error = "UserId is required" });

            if (request.Attempts == null || request.Attempts.Count == 0)
                return BadRequest(new { error = "At least one attempt is required" });

            // Validar que todas as tentativas têm dados necessários
            if (request.Attempts.Any(a => string.IsNullOrEmpty(a.TargetEmotion) || a.Images == null || a.Images.Count == 0))
                return BadRequest(new { error = "All attempts must have targetEmotion and at least one image in images array" });

            var response = await _emotionService.BatchAnalyzeAsync(request);

            _logger.LogInformation("Batch analysis concluído para sessão {SessionId}: {Correct}/{Total} corretos ({Accuracy:P2})",
                response.SessionId, response.Correct, response.Total, response.Accuracy);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Erro de validação no batch analysis: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao processar batch analysis: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Erro ao processar batch analysis", details = ex.Message });
        }
    }
}
