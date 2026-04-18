namespace EMOPlay.Application.DTOs.Game;

/// <summary>
/// Requisição para salvar os resultados de um jogo completo (formato do Frontend)
/// </summary>
public class SaveGameResultRequest
{
    public required Guid ChildId { get; set; }
    public string? SessionId { get; set; } // ID da sessão do frontend (opcional)
    public int? PhaseNumber { get; set; } // Número da fase (opcional)
    public required string GameMode { get; set; } // "identify" ou "make-emotion"
    public required string Level { get; set; } // "easy", "medium", "hard"
    public required List<ChallengeResult> Results { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required int TotalScore { get; set; } // Número de acertos
    public required int TotalChallenges { get; set; } // Total de desafios

    // Propriedade calculada para compatibilidade com a lógica existente
    public long TotalDuration => (long)(EndTime - StartTime).TotalMilliseconds;
    
    // Propriedade calculada para obter acuracyRate
    public double AccuracyRate => TotalChallenges > 0 ? (TotalScore * 100.0 / TotalChallenges) : 0;
}

/// <summary>
/// Resultado de um desafio individual
/// </summary>
public class ChallengeResult
{
    public int? ChallengeNumber { get; set; } // Opcional
    public required string TargetEmotion { get; set; }
    public required string DetectedEmotion { get; set; }
    public required bool IsCorrect { get; set; }
    public required long ResponseTime { get; set; } // em millisegundos
    public required DateTime Timestamp { get; set; }
}

/// <summary>
/// Estatísticas consolidadas do jogo
/// </summary>
public class GameStatistics
{
    public required int TotalChallenges { get; set; }
    public required int CorrectAnswers { get; set; }
    public required double AccuracyRate { get; set; } // em percentual (0-100)
}
