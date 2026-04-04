namespace EMOPlay.Domain.Entities;

public class SessionResult
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalChallenges { get; set; }
    public double Percentage { get; set; }
    public string Message { get; set; }
    public string ResultsJson { get; set; } // Armazena os resultados individuais em JSON
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual GameSession? GameSession { get; set; }
}
