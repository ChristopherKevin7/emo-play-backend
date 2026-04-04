using EMOPlay.Application.DTOs.Metrics;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMetricsService metricsService, ILogger<MetricsController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém métricas consolidadas de desempenho de uma criança
    /// </summary>
    /// <remarks>
    /// Retorna um relatório detalhado com estatísticas de desempenho de uma criança.
    /// Dados são agregados de todas as sessões de jogo e podem ser filtrados por período.
    /// **Este endpoint é especializado para psicólogos gerarem relatórios e análises.**
    /// 
    /// **Métrica retornadas:**
    /// - totalSessions: Número de sessões jogadas
    /// - accuracyRate: Taxa geral de acurácia (0-1)
    /// - averageResponseTime: Tempo médio de resposta em ms
    /// - averageConfidence: Confiança média da IA
    /// - emotionBreakdown: Detalhes por emoção (tentativas, acertos, taxa de acurácia)
    /// - progressTrend: Análise de progresso ao longo do tempo
    /// 
    /// **Parâmetros de filtro (opcionais):**
    /// - sessionId: Se fornecido, retorna dados de sessão específica
    /// - startDate: Data início do período (formato: YYYY-MM-DD)
    /// - endDate: Data fim do período (formato: YYYY-MM-DD)
    /// 
    /// **Exemplo de breakdown por emoção:**
    /// ```json
    /// {
    ///   "happy": {
    ///     "attempts": 8,
    ///     "correctAttempts": 7,
    ///     "accuracy": 0.875,
    ///     "avgConfidence": 0.88
    ///   }
    /// }
    /// ```
    /// 
    /// **Útil para:**
    /// - Avaliar progresso da criança
    /// - Identificar emoções com dificuldade
    /// - Planejar intervenções educacionais
    /// - Gerar relatórios para pais/responsáveis
    /// </remarks>
    /// <param name="childId">ID (Guid) da criança (obrigatório)</param>
    /// <param name="sessionId">ID (Guid) de uma sessão específica (opcional)</param>
    /// <param name="startDate">Data de início para filtro de período (opcional)</param>
    /// <param name="endDate">Data de fim para filtro de período (opcional)</param>
    /// <returns>Relatório completo de métricas e análises</returns>
    /// <response code="200">Métricas retornadas com sucesso</response>
    /// <response code="400">ChildId não encontrado ou datas em formato incorreto</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(ChildMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildMetricsResponse>> GetChildMetrics(
        [FromRoute] Guid childId,
        [FromQuery] Guid? sessionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation($"Getting metrics for child {childId}");
            var response = await _metricsService.GetChildMetricsAsync(childId, sessionId, startDate, endDate);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting metrics: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
