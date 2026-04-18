using EMOPlay.Application.DTOs.Emotion;

namespace EMOPlay.Application.Interfaces;

/// <summary>
/// Interface para serviços relacionados a análise de emoções
/// </summary>
public interface IEmotionService
{
    /// <summary>
    /// Processa múltiplas imagens em batch, enviando cada uma para análise via IA
    /// </summary>
    /// <param name="request">Requisição contendo userId, sessionId e lista de tentativas</param>
    /// <returns>Resposta consolidada com resultados individuais e estatísticas gerais</returns>
    Task<BatchAnalyzeResponse> BatchAnalyzeAsync(BatchAnalyzeRequest request);
}
