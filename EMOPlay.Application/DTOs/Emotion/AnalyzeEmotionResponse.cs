namespace EMOPlay.Application.DTOs.Emotion;

public class AnalyzeEmotionResponse
{
    public required string DetectedEmotion { get; set; }
    public double Confidence { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnalysisTimestamp { get; set; }
}
