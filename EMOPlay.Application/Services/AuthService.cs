using EMOPlay.Application.DTOs.Auth;
using EMOPlay.Application.Interfaces;
using EMOPlay.Domain.Entities;
using EMOPlay.Domain.Enums;
using EMOPlay.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace EMOPlay.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Convert string role to RoleEnum
        if (!Enum.TryParse<RoleEnum>(request.Role, ignoreCase: true, out var requestedRole))
            throw new UnauthorizedAccessException("Role inválida. Use 'child' ou 'psychologist'.");

        // Find user by email, active status, AND role
        var users = await _unitOfWork.Users
            .FindAsync(u => u.Email == request.Email && u.IsActive && u.Role == requestedRole);

        var user = users.FirstOrDefault();
        if (user == null)
            throw new UnauthorizedAccessException("Email, senha ou role inválidos.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email, senha ou role inválidos.");

        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = (int)user.Role,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada.");
        var issuer = jwtSettings["Issuer"] ?? "EMOPlay";
        var audience = jwtSettings["Audience"] ?? "EMOPlay";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", user.Id.ToString()),
            new System.Security.Claims.Claim("email", user.Email),
            new System.Security.Claims.Claim("name", user.Name),
            new System.Security.Claims.Claim("role", ((int)user.Role).ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string password, string hash)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHash = Convert.ToBase64String(hashedBytes);
            return computedHash == hash;
        }
    }
}
