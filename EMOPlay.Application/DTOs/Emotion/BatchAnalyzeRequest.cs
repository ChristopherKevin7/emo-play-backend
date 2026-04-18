namespace EMOPlay.Application.DTOs.Emotion;

/// <summary>
/// Requisição para processar múltiplas imagens em batch
/// </summary>
public class BatchAnalyzeRequest
{
    /// <summary>
    /// ID do usuário (criança) que está realizando as tentativas
    /// </summary>
    public required string UserId { get; set; }
    
    /// <summary>
    /// ID da sessão de jogo (opcional - será gerado pelo backend se não fornecido)
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Lista de tentativas a serem analisadas
    /// </summary>
    public required List<BatchAttempt> Attempts { get; set; }
}
