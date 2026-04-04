using EMOPlay.Application.DTOs.User;
using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Entities;
using EMOPlay.Domain.Enums;
using EMOPlay.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EMOPlay.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        // Validar duplicação de email
        var existingUser = await _unitOfWork.Users
            .FindAsync(u => u.Email == request.Email);

        if (existingUser.Any())
            throw new InvalidOperationException($"Usuário com email '{request.Email}' já existe.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = (RoleEnum)request.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid userId)
    {
        var user = await _unitOfWork.Users
            .GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID '{userId}' não encontrado.");

        return MapToResponse(user);
    }

    public async Task<UserResponse> GetUserByEmailAsync(string email)
    {
        var users = await _unitOfWork.Users
            .FindAsync(u => u.Email == email);

        var user = users.FirstOrDefault();
        if (user == null)
            throw new KeyNotFoundException($"Usuário com email '{email}' não encontrado.");

        return MapToResponse(user);
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users
            .GetAllAsync();

        return users.Select(MapToResponse).ToList();
    }

    public async Task<List<UserResponse>> GetChildrenAsync()
    {
        var children = await _unitOfWork.Users
            .FindAsync(u => (int)u.Role == 1 && u.IsActive);

        return children.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users
            .GetByIdAsync(request.Id);

        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID '{request.Id}' não encontrado.");

        // Validar novo email se foi alterado
        if (user.Email != request.Email)
        {
            var existingEmail = await _unitOfWork.Users
                .FindAsync(u => u.Email == request.Email && u.Id != request.Id);

            if (existingEmail.Any())
                throw new InvalidOperationException($"Email '{request.Email}' já está em uso.");
        }

        user.Name = request.Name;
        user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = HashPassword(request.Password);

        if (request.Role.HasValue)
            user.Role = (RoleEnum)request.Role.Value;

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users
            .GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID '{userId}' não encontrado.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        var users = await _unitOfWork.Users
            .FindAsync(u => u.Email == email);

        return users.Any();
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = (int)user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
