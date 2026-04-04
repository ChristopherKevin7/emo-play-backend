namespace EMOPlay.Application.DTOs.Game;

public class EndGameSessionRequest
{
    public Guid SessionId { get; set; }
    public int TotalPoints { get; set; }
    public int TotalChallenges { get; set; }
    public double AccuracyRate { get; set; }
}
