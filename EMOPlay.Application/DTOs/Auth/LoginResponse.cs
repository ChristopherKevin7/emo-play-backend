namespace EMOPlay.Application.DTOs.Auth;

public class LoginResponse
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public int Role { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
