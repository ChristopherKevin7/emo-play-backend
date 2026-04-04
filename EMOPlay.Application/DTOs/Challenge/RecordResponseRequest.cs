namespace EMOPlay.Application.DTOs.Challenge;

public class RecordResponseRequest
{
    public Guid SessionId { get; set; }
    public Guid ChallengeId { get; set; }
    public string TargetEmotion { get; set; }
    public string ChildResponse { get; set; }
    public bool IsCorrect { get; set; }
    public int ResponseTimeMs { get; set; }
    public double Confidence { get; set; }
    public string ImageUrl { get; set; }
}
