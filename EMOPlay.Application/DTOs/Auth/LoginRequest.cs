namespace EMOPlay.Application.DTOs.Auth;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; } // "child" or "psychologist"
}
