using EMOPlay.Application.DTOs.Emotion;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmotionsController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<EmotionsController> _logger;

    public EmotionsController(IAIService aiService, ILogger<EmotionsController> logger)
    {
        _aiService = aiService;
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

            // Call AI service
            var aiResult = await _aiService.AnalyzeEmotionAsync(request.Image, request.TargetEmotion);

            var isCorrect = aiResult.DetectedEmotion.Equals(request.TargetEmotion, StringComparison.OrdinalIgnoreCase);

            var response = new AnalyzeEmotionResponse
            {
                DetectedEmotion = aiResult.DetectedEmotion,
                Confidence = aiResult.Confidence,
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
}
