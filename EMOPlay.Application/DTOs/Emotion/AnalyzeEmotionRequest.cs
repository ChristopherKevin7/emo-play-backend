namespace EMOPlay.Application.DTOs.Emotion;

public class AnalyzeEmotionRequest
{
    public Guid SessionId { get; set; }
    public required string Image { get; set; } // Base64-encoded image
    public required string TargetEmotion { get; set; }
    public Guid ChallengeId { get; set; }
}
