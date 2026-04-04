using EMOPlay.Domain.Enums;

namespace EMOPlay.Domain.Entities;

public class Challenge
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public EmotionEnum TargetEmotion { get; set; }
    public EmotionEnum? ChildResponse { get; set; }
    public bool IsCorrect { get; set; }
    public int ResponseTimeMs { get; set; }
    public double Confidence { get; set; }
    public required string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual GameSession? GameSession { get; set; }
}
