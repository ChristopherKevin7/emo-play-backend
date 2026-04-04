namespace EMOPlay.Application.DTOs.Game;

public class EndGameSessionResponse
{
    public Guid SessionId { get; set; }
    public required string Status { get; set; }
    public required EndGameSessionSummary Summary { get; set; }
    public required string Message { get; set; }
}

public class EndGameSessionSummary
{
    public int TotalPoints { get; set; }
    public int TotalChallenges { get; set; }
    public double AccuracyRate { get; set; }
    public int AverageResponseTimeMs { get; set; }
}
