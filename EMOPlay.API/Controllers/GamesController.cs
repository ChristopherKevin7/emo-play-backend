using EMOPlay.Application.DTOs.Game;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Salva os resultados de um jogo completo da criança
    /// </summary>
    /// <remarks>
    /// Recebe e armazena os dados completos de uma sessão de jogo, incluindo
    /// todas as respostas da criança, tempos de resposta, emoções detectadas
    /// e estatísticas consolidadas.
    /// 
    /// **Dados da requisição:**
    /// - childId: ID da criança
    /// - gameMode: "identify" ou "make-emotion"
    /// - levelId: "easy", "medium" ou "hard"
    /// - startTime/endTime: Timestamps da sessão
    /// - totalDuration: Duração total em millisegundos
    /// - results: Array com detalhe de cada desafio (challengeNumber, targetEmotion, detectedEmotion, isCorrect, responseTime, timestamp)
    /// - statistics: Resumo com totalChallenges, correctAnswers, accuracyRate
    /// 
    /// **Fluxo:**
    /// 1. Recebe os dados consolidados do jogo
    /// 2. Cria um registro de sessão de jogo
    /// 3. Salva todos os desafios individuais
    /// 4. Armazena as estatísticas
    /// 5. Retorna confirmação e ID da sessão criada
    /// 
    /// **Estes dados são usados para:**
    /// - Gerar relatórios do psicólogo
    /// - Análise de progresso da criança
    /// - Feedback e intervenção educacional
    /// - Rastreamento histórico de performance
    /// </remarks>
    /// <param name="request">Dados consolidados do jogo completo</param>
    /// <returns>Confirmação de salvamento e ID da sessão</returns>
    /// <response code="200">Resultados salvos com sucesso</response>
    /// <response code="400">Dados inválidos ou childId não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("results")]
    [ProducesResponseType(typeof(SessionResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionResultResponse>> SaveGameResult([FromBody] SaveGameResultRequest request)
    {
        try
        {
            _logger.LogInformation($"Saving game results for child {request.ChildId}. Mode: {request.GameMode}, Level: {request.LevelId}, Accuracy: {request.Statistics.AccuracyRate}%");
            var response = await _gameService.SaveGameResultAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving game results: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Recupera os resultados consolidados de uma sessão específica
    /// </summary>
    /// <remarks>
    /// Busca e retorna os resultados salvos de uma sessão de jogo.
    /// Permite ao psicólogo ou sistema revisar o desempenho consolidado de uma sessão.
    /// 
    /// **Retorna:**
    /// - sessionId: ID da sessão
    /// - childId: ID da criança
    /// - acertos: Quantidade de acertos
    /// - percentage: Percentual de acurácia
    /// - message: Feedback consolidado
    /// - resultados: Array com histórico de cada desafio
    /// - timestamps: Data de criação dos resultados
    /// 
    /// **Útil para:**
    /// - Revisar performance após sessão
    /// - Gerar relatórios
    /// - Análise de progresso longitudinal
    /// </remarks>
    /// <param name="sessionId">ID (Guid) da sessão cujos resultados buscar</param>
    /// <returns>Dados consolidados da sessão</returns>
    /// <response code="200">Resultados retornados com sucesso</response>
    /// <response code="400">SessionId não encontrado ou inválido</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("results/{sessionId}")]
    [ProducesResponseType(typeof(SessionResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionResultResponse>> GetSessionResult([FromRoute] Guid sessionId)
    {
        try
        {
            _logger.LogInformation($"Getting session results for {sessionId}");
            var response = await _gameService.GetSessionResultAsync(sessionId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting session results: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
