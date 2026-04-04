namespace EMOPlay.Application.DTOs.User;

public class CreateUserRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public int Role { get; set; } // 1 = Child, 2 = Psychologist
}
