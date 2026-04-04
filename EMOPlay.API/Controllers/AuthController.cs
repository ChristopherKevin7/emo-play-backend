using EMOPlay.Application.DTOs.Auth;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica um usuário com email, senha e role, retornando um token JWT
    /// </summary>
    /// <remarks>
    /// Realiza a autenticação do usuário validando as credenciais fornecidas (email, senha e role).
    /// A validação de role garante que somente usuários com a role correta possam fazer login.
    /// Se as credenciais forem válidas E a role corresponder, retorna um token JWT que deve ser 
    /// usado nos headers das requisições subsequentes (Authorization: Bearer {token}).
    /// O token tem validade de 24 horas por padrão.
    /// 
    /// **Fluxo de Validação:**
    /// 1. Recebe email, senha e role ("child" ou "psychologist") do usuário
    /// 2. Busca o usuário no banco de dados pelo email E role especificada
    /// 3. Se não encontrar usuário com aquela role, retorna erro 401
    /// 4. Verifica se a senha (hash) corresponde
    /// 5. Se válido, gera um token JWT com informações do usuário
    /// 6. Retorna token, dados do usuário e data de expiração
    /// 
    /// **Exemplos:**
    /// - Login de criança com role="child" → sucesso (se credenciais válidas)
    /// - Login de criança com role="psychologist" → erro 401
    /// - Login de psicólogo com role="psychologist" → sucesso (se credenciais válidas)
    /// - Login de psicólogo com role="child" → erro 401
    /// </remarks>
    /// <param name="request">Objeto contendo email, senha e role do usuário</param>
    /// <returns>LoginResponse com token JWT e informações do usuário autenticado</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="401">Email, senha ou role inválidos</response>
    /// <response code="400">Dados inválidos na requisição</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            
            var response = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User logged in successfully with ID: {UserId}", response.UserId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao fazer login" });
        }
    }
}
