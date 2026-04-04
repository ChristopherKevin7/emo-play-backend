namespace EMOPlay.Application.DTOs.Game;

public class StartGameSessionResponse
{
    public Guid SessionId { get; set; }
    public required string GameMode { get; set; }
    public required string Status { get; set; }
    public DateTime Timestamp { get; set; }
}
