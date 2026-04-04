using EMOPlay.Application.DTOs.User;

namespace EMOPlay.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> GetUserByIdAsync(Guid userId);
    Task<UserResponse> GetUserByEmailAsync(string email);
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<List<UserResponse>> GetChildrenAsync();
    Task<UserResponse> UpdateUserAsync(UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> UserExistsByEmailAsync(string email);
}
