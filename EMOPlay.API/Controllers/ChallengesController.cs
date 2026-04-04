using EMOPlay.Application.DTOs.Challenge;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challengeService;
    private readonly ILogger<ChallengesController> _logger;

    public ChallengesController(IChallengeService challengeService, ILogger<ChallengesController> logger)
    {
        _challengeService = challengeService;
        _logger = logger;
    }

    /// <summary>
    /// Registra a resposta de uma criança a um desafio de emoção
    /// </summary>
    /// <remarks>
    /// Armazena e processa a resposta de uma criança a um desafio específico.
    /// Calcula pontuação, valida acertos e mantém histórico para análise.
    /// 
    /// **Fluxo:**
    /// 1. Recebe sessionId, challengeId, emoção alvo, resposta da criança
    /// 2. Valida se a resposta está correta (comparando com targetEmotion)
    /// 3. Calcula pontos baseado em acerto/erro
    /// 4. Armazena temporizador (tempo de resposta)
    /// 5. Mantém score de confiança da IA
    /// 6. Armazena imagem capturada para referência
    /// 
    /// **Pontuação:**
    /// - Resposta correta: +10 pontos (base)
    /// - Resposta incorreta: 0 pontos
    /// - Bônus: Pontos adicionais por tempo rápido/confiança alta
    /// 
    /// **Dados salvos:**
    /// - targetEmotion: O que a criança deveria mostrar
    /// - childResponse: O que a criança respondeu
    /// - isCorrect: Se acertou
    /// - responseTime: Tempo em milissegundos
    /// - confidence: Confiança da detecção de IA (0-1)
    /// - imageUrl: Referência da imagem capturada
    /// </remarks>
    /// <param name="request">Dados do desafio: sessionId, challengeId, targetEmotion, childResponse, isCorrect, responseTime, confidence, imageUrl</param>
    /// <returns>Confirmação com pontos ganhos e total acumulado</returns>
    /// <response code="200">Resposta registrada com sucesso</response>
    /// <response code="400">Dados inválidos ou sessionId/challengeId não encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("record-response")]
    [ProducesResponseType(typeof(RecordResponseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecordResponseResponse>> RecordResponse([FromBody] RecordResponseRequest request)
    {
        try
        {
            _logger.LogInformation($"Recording response for challenge {request.ChallengeId}");
            var response = await _challengeService.RecordResponseAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recording response: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
