using EMOPlay.Application.DTOs.Game;

namespace EMOPlay.Application.Interfaces;

public interface IGameService
{
    /// <summary>
    /// Salva os resultados de um jogo completo da criança
    /// </summary>
    Task<SessionResultResponse> SaveGameResultAsync(SaveGameResultRequest request);
    
    /// <summary>
    /// Recupera os resultados de uma sessão específica
    /// </summary>
    Task<SessionResultResponse> GetSessionResultAsync(Guid sessionId);
}
