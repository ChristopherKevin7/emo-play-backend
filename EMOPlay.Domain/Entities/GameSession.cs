using EMOPlay.Domain.Enums;

namespace EMOPlay.Domain.Entities;

public class GameSession
{
    public Guid Id { get; set; }
    public Guid ChildId { get; set; }
    public GameModeEnum GameMode { get; set; }
    public GameSessionStatusEnum Status { get; set; }
    public int TotalPoints { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public double? AccuracyRate { get; set; }
    public string Level { get; set; } = "easy"; // easy, medium, hard

    // Navigation properties
    public virtual Child? Child { get; set; }
    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
}
