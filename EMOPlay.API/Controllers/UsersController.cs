using EMOPlay.Application.DTOs.User;
using EMOPlay.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMOPlay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IScoreService _scoreService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IScoreService scoreService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _scoreService = scoreService;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo usuário (criança ou psicólogo)
    /// </summary>
    /// <remarks>
    /// Cria um novo usuário no sistema com email único. Valida se o email já existe
    /// antes de criar o usuário. A senha é automaticamente criptografada usando SHA256.
    /// 
    /// **Roles disponíveis:**
    /// - 1 = Child (criança que usa a plataforma)
    /// - 2 = Psychologist (psicólogo que gerencia as crianças)
    /// 
    /// **Fluxo:**
    /// 1. Recebe dados do novo usuário (nome, email, senha, role)
    /// 2. Valida se o email já existe
    /// 3. Criptografa a senha com SHA256
    /// 4. Salva o usuário no banco de dados
    /// 5. Retorna os dados do usuário criado (sem a senha)
    /// 
    /// **Este endpoint NÃO requer autenticação** para permitir que novos usuários se registrem.
    /// </remarks>
    /// <param name="request">Dados do novo usuário (nome, email, senha, role)</param>
    /// <returns>UserResponse com os dados do usuário criado</returns>
    /// <response code="201">Usuário criado com sucesso</response>
    /// <response code="400">Email já existe ou dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new user with email: {Email}", request.Email);
            
            var response = await _userService.CreateUserAsync(request);
            
            _logger.LogInformation("User created successfully with ID: {UserId}", response.Id);
            return CreatedAtAction(nameof(GetUserById), new { userId = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error creating user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao criar usuário" });
        }
    }

    /// <summary>
    /// Busca um usuário pelo ID (requer autenticação)
    /// </summary>
    /// <remarks>
    /// Retorna os dados de um usuário específico usando seu ID (Guid).
    /// Requer um token JWT válido no header Authorization.
    /// 
    /// **Header obrigatório:**
    /// ```
    /// Authorization: Bearer {seu_token_jwt}
    /// ```
    /// </remarks>
    /// <param name="userId">ID (Guid) do usuário a buscar</param>
    /// <returns>Dados do usuário encontrado</returns>
    /// <response code="200">Usuário encontrado com sucesso</response>
    /// <response code="404">Usuário não encontrado</response>
    /// <response code="401">Token não fornecido ou inválido</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize]
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] Guid userId)
    {
        try
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", userId);
            
            var response = await _userService.GetUserByIdAsync(userId);
            
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao buscar usuário" });
        }
    }

    /// <summary>
    /// Busca um usuário pelo email (requer autenticação)
    /// </summary>
    /// <remarks>
    /// Localiza e retorna um usuário pelo seu email.
    /// Requer um token JWT válido no header Authorization.
    /// 
    /// **Useful para:**
    /// - Verificar informações de um usuário específico
    /// - Obter detalhes do perfil de um usuário
    /// </remarks>
    /// <param name="email">Email do usuário a buscar</param>
    /// <returns>Dados do usuário com esse email</returns>
    /// <response code="200">Usuário encontrado com sucesso</response>
    /// <response code="404">Usuário com esse email não encontrado</response>
    /// <response code="401">Token não fornecido ou inválido</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize]
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> GetUserByEmail([FromRoute] string email)
    {
        try
        {
            _logger.LogInformation("Fetching user with email: {Email}", email);
            
            var response = await _userService.GetUserByEmailAsync(email);
            
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao buscar usuário" });
        }
    }

    /// <summary>
    /// Lista todos os usuários do sistema
    /// </summary>
    /// <remarks>
    /// Retorna uma lista com todos os usuários cadastrados no sistema (crianças e psicólogos).
    /// Sem filtro de autenticação para permitir consultas gerais.
    /// 
    /// **Retorna:**
    /// - ID, nome, email, role de cada usuário
    /// - Status ativo/inativo
    /// - Datas de criação e atualização
    /// </remarks>
    /// <returns>Lista de todos os usuários cadastrados</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Fetching all users");
            
            var response = await _userService.GetAllUsersAsync();
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching users");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao buscar usuários" });
        }
    }

    /// <summary>
    /// Lista todas as crianças cadastradas (apenas psicólogos)
    /// </summary>
    /// <remarks>
    /// Retorna uma lista com todos os usuários que têm role "Child" (role = 1).
    /// **Este endpoint é restrito apenas para psicólogos (role = 2).**
    /// Requer um token JWT válido de um psicólogo.
    /// 
    /// **Fluxo de segurança:**
    /// 1. Valida se o token é de um psicólogo (role = 2)
    /// 2. Se for outro role, retorna 403 Forbidden
    /// 3. Se for psicólogo, retorna lista de todas as crianças ativas
    /// 
    /// **Retorna:**
    /// - Nome, email, role da criança
    /// - ID e datas de criação
    /// - Status ativo/inativo
    /// </remarks>
    /// <returns>Lista de usuários com role Child</returns>
    /// <response code="200">Lista de crianças retornada com sucesso</response>
    /// <response code="401">Token não fornecido ou inválido</response>
    /// <response code="403">Acesso proibido - apenas psicólogos podem acessar este endpoint</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize]
    [HttpGet("children")]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UserResponse>>> GetChildren()
    {
        try
        {
            // Validate that user is a Psychologist (role = 2)
            var roleClaim = User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "2")
            {
                _logger.LogWarning("Unauthorized access attempt to GetChildren by non-psychologist user. Role: {Role}", roleClaim?.Value ?? "NULL");
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Apenas psicólogos podem acessar essa rota." });
            }

            _logger.LogInformation("Fetching all children");
            
            var response = await _userService.GetChildrenAsync();
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching children");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao buscar crianças" });
        }
    }

    /// <summary>
    /// Atualiza os dados de um usuário (requer autenticação)
    /// </summary>
    /// <remarks>
    /// Atualiza informações de um usuário existente. Permite modificar:
    /// - Nome
    /// - Email (com validação de unicidade)
    /// - Senha (será re-criptografada)
    /// - Role (só admin deveria fazer isso)
    /// 
    /// Requer um token JWT válido no header Authorization.
    /// 
    /// **Fluxo:**
    /// 1. Valida se o usuário a atualizar existe
    /// 2. Se mudar email, verifica se novo email não está em uso
    /// 3. Se fornecer nova senha, re-criptografa com SHA256
    /// 4. Atualiza o registro no banco de dados
    /// 5. Retorna os dados atualizados
    /// </remarks>
    /// <param name="request">Dados a atualizar (Id obrigatório, demais campos opcionais)</param>
    /// <returns>Dados atualizados do usuário</returns>
    /// <response code="200">Usuário atualizado com sucesso</response>
    /// <response code="400">Email já em uso ou dados inválidos</response>
    /// <response code="404">Usuário não encontrado</response>
    /// <response code="401">Token não fornecido ou inválido</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", request.Id);
            
            var response = await _userService.UpdateUserAsync(request);
            
            _logger.LogInformation("User updated successfully with ID: {UserId}", response.Id);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error updating user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao atualizar usuário" });
        }
    }

    /// <summary>
    /// Deleta um usuário (soft delete - apenas marca como inativo)
    /// </summary>
    /// <remarks>
    /// Realiza a exclusão lógica de um usuário, marcando-o como inativo (IsActive = false)
    /// sem deletar os dados do banco de dados. Isso preserva a integridade referencial
    /// dos dados históricos (sessões, desafios, etc).
    /// 
    /// Requer um token JWT válido no header Authorization.
    /// 
    /// **O que acontece:**
    /// 1. Busca o usuário pelo ID
    /// 2. Marca como IsActive = false
    /// 3. Atualiza o timestamp de atualização
    /// 4. Retorna 204 No Content
    /// 
    /// **Nota:**
    /// - O usuário será invisível em listas (filtradas por IsActive = true)
    /// - Os dados históricos (sessões, desafios) continuam no banco
    /// - Pode ser reativado manualmente no banco se necessário
    /// </remarks>
    /// <param name="userId">ID (Guid) do usuário a deletar</param>
    /// <returns>Sem conteúdo (204 No Content)</returns>
    /// <response code="204">Usuário deletado com sucesso</response>
    /// <response code="404">Usuário não encontrado</response>
    /// <response code="401">Token não fornecido ou inválido</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize]
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", userId);
            
            await _userService.DeleteUserAsync(userId);
            
            _logger.LogInformation("User deleted successfully with ID: {UserId}", userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao deletar usuário" });
        }
    }

    /// <summary>
    /// Verifica se um email já existe no sistema
    /// </summary>
    /// <remarks>
    /// Endpoint leve (HEAD request) que verifica se um email está registrado.
    /// Retorna apenas headers sem corpo de resposta:
    /// - 200 OK = Email existe
    /// - 404 Not Found = Email não existe
    /// 
    /// **Útil para:**
    /// - Validação em tempo real no cadastro
    /// - Verificar disponibilidade de email antes de criar usuário
    /// </remarks>
    /// <param name="email">Email a verificar</param>
    /// <returns>Headers de status sem corpo</returns>
    /// <response code="200">Email existe no sistema</response>
    /// <response code="404">Email não encontrado/disponível</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpHead("exists/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UserExists([FromRoute] string email)
    {
        try
        {
            var exists = await _userService.UserExistsByEmailAsync(email);
            
            if (exists)
                return Ok();
            
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking if user exists");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Retorna a pontuação acumulada do usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>userId e pontuação total</returns>
    /// <response code="200">Pontuação retornada com sucesso</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpGet("{userId}/points")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPoints([FromRoute] Guid userId)
    {
        try
        {
            var points = await _scoreService.GetUserPointsAsync(userId);
            return Ok(new { userId, points });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found when fetching points: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user points for {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao buscar pontuação" });
        }
    }
}
