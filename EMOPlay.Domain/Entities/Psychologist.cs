namespace EMOPlay.Domain.Entities;

public class Psychologist
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string CRP { get; set; } // Conselho Regional de Psicologia
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual ICollection<Child> Children { get; set; } = new List<Child>();
}
