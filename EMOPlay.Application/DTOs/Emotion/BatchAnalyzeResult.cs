namespace EMOPlay.Application.DTOs.Emotion;

/// <summary>
/// Resultado consolidado da análise de uma emoção (agregação de múltiplos frames)
/// </summary>
public class BatchAnalyzeResult
{
    /// <summary>
    /// Emoção alvo
    /// </summary>
    public required string TargetEmotion { get; set; }
    
    /// <summary>
    /// Top predições consolidadas (médias dos frames), ordenadas por score decrescente
    /// </summary>
    public List<EmotionPrediction> TopPredictions { get; set; } = new();
    
    /// <summary>
    /// Scores médios por emoção (média entre todos os frames)
    /// </summary>
    public Dictionary<string, double> AverageScores { get; set; } = new();
    
    /// <summary>
    /// Indica se a emoção foi considerada correta (ver regras de avaliação)
    /// </summary>
    public bool IsCorrect { get; set; }
    
    /// <summary>
    /// Timestamp da análise
    /// </summary>
    public DateTime AnalysisTimestamp { get; set; }
}

/// <summary>
/// Predição individual de emoção com score
/// </summary>
public class EmotionPrediction
{
    public required string Emotion { get; set; }
    public double Score { get; set; }
}
