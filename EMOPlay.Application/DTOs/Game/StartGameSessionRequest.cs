namespace EMOPlay.Application.DTOs.Game;

public class StartGameSessionRequest
{
    public Guid ChildId { get; set; }
    public required string GameMode { get; set; } // "identify-emotion" | "make-emotion"
    public Guid PsychologistId { get; set; }
}
