namespace EMOPlay.Application.DTOs.Emotion;

/// <summary>
/// Representa uma tentativa individual no batch de análise (múltiplos frames por emoção)
/// </summary>
public class BatchAttempt
{
    /// <summary>
    /// Emoção alvo que a criança deveria expressar
    /// </summary>
    public required string TargetEmotion { get; set; }
    
    /// <summary>
    /// Lista de imagens em base64 (múltiplos frames capturados)
    /// </summary>
    public required List<string> Images { get; set; }
}
