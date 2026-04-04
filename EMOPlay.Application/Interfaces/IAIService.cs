using EMOPlay.Application.DTOs.Emotion;

namespace EMOPlay.Application.Interfaces;

public interface IAIService
{
    Task<AIEmotionAnalysisResult> AnalyzeEmotionAsync(string base64Image, string targetEmotion);
}

public class AIEmotionAnalysisResult
{
    public string DetectedEmotion { get; set; }
    public double Confidence { get; set; }
}
