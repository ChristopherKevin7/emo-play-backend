namespace EMOPlay.Application.DTOs.Emotion;

/// <summary>
/// Resposta consolidada da análise em batch
/// </summary>
public class BatchAnalyzeResponse
{
    /// <summary>
    /// ID da sessão processada (gerado pelo backend)
    /// </summary>
    public required string SessionId { get; set; }
    
    /// <summary>
    /// Total de tentativas processadas
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Total de tentativas consideradas corretas
    /// </summary>
    public int Correct { get; set; }
    
    /// <summary>
    /// Pontuação calculada para esta sessão (acertos × 50 pts)
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// Taxa de acerto (0-1)
    /// </summary>
    public double Accuracy { get; set; }
    
    /// <summary>
    /// Resultados detalhados de cada tentativa
    /// </summary>
    public List<BatchAnalyzeResult> Results { get; set; } = new();
    
    /// <summary>
    /// Timestamp do processamento
    /// </summary>
    public DateTime ProcessedAt { get; set; }
    
    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public required string Message { get; set; }
}
