namespace EMOPlay.Domain.Entities;

public class Child
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int AgeRange { get; set; }
    public Guid PsychologistId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    public virtual Psychologist? Psychologist { get; set; }
}
