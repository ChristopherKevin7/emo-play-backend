namespace EMOPlay.Application.Interfaces;

public interface IScoreService
{
    /// <summary>
    /// Calcula a pontuação do Desafio 1 (identificar emoções) com base em acertos e nível.
    /// easy = 10 pts/acerto | medium = 20 pts/acerto | hard = 30 pts/acerto
    /// </summary>
    int CalculateDesafio1Score(int correctCount, string level);

    /// <summary>
    /// Calcula a pontuação do Desafio 2 (expressar emoções).
    /// 50 pts por acerto, independente de nível.
    /// </summary>
    int CalculateDesafio2Score(int correctCount);

    /// <summary>
    /// Incrementa o campo Points do usuário no banco de dados.
    /// </summary>
    Task AddUserPointsAsync(Guid userId, int points);

    /// <summary>
    /// Retorna a pontuação atual do usuário.
    /// </summary>
    Task<int> GetUserPointsAsync(Guid userId);
}
