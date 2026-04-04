using EMOPlay.Application.DTOs.Auth;

namespace EMOPlay.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
