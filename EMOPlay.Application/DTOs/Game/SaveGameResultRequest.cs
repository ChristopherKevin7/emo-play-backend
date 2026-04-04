namespace EMOPlay.Application.DTOs.Game;

/// <summary>
/// Requisição para salvar os resultados de um jogo completo
/// </summary>
public class SaveGameResultRequest
{
    public required Guid ChildId { get; set; }
    public required string GameMode { get; set; } // "identify" ou "make-emotion"
    public required string LevelId { get; set; } // "easy", "medium", "hard"
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required long TotalDuration { get; set; } // em millisegundos
    public required List<ChallengeResult> Results { get; set; }
    public required GameStatistics Statistics { get; set; }
}

/// <summary>
/// Resultado de um desafio individual
/// </summary>
public class ChallengeResult
{
    public required int ChallengeNumber { get; set; }
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
