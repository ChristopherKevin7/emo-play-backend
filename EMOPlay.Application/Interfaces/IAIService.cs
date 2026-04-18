using System.Text.Json.Serialization;

namespace EMOPlay.Application.Interfaces;

public interface IAIService
{
    /// <summary>
    /// Envia múltiplas imagens para o módulo de IA e recebe predições com top emoções
    /// </summary>
    Task<AIBatchAnalysisResult> AnalyzeBatchAsync(List<string> base64Images, string targetEmotion);
}

/// <summary>
/// Resultado da análise do módulo de IA contendo lista de predições
/// </summary>
public class AIBatchAnalysisResult
{
    [JsonPropertyName("predictions")]
    public List<AIEmotionPrediction> Predictions { get; set; } = new();
}

/// <summary>
/// Predição individual retornada pelo módulo de IA
/// </summary>
public class AIEmotionPrediction
{
    [JsonPropertyName("emotion")]
    public string Emotion { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public double Score { get; set; }
}
