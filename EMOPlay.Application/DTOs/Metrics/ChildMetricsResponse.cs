namespace EMOPlay.Application.DTOs.Metrics;

/// <summary>
/// Resposta consolidada com métricas agrupadas por desafio
/// </summary>
public class ChildMetricsResponse
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; }
    
    /// <summary>
    /// Métricas do Desafio 1 - Identificação de emoções
    /// </summary>
    public Desafio1MetricsData? Desafio_1 { get; set; }
    
    /// <summary>
    /// Métricas do Desafio 2 - Expressar emoções com confiança
    /// </summary>
    public Desafio2MetricsData? Desafio_2 { get; set; }
    
    public string Disclaimer { get; set; }
}

/// <summary>
/// Dados de métricas para o Desafio 1 - Identificação de emoções (sem confiança)
/// </summary>
public class Desafio1MetricsData
{
    public int TotalSessions { get; set; }
    public double AccuracyRate { get; set; }
    public int AverageResponseTimeMs { get; set; }
    public Dictionary<string, Desafio1EmotionBreakdown> EmotionBreakdown { get; set; }
    public List<ProgressTrendItem> ProgressTrend { get; set; }
    
    /// <summary>
    /// Histórico de todas as tentativas ordenadas por data
    /// </summary>
    public List<Desafio1HistoricAttempt> HistoricAttempts { get; set; }
}

/// <summary>
/// Dados de métricas para o Desafio 2 - Expressar emoções (modelo probabilístico)
/// </summary>
public class Desafio2MetricsData
{
    public int TotalSessions { get; set; }
    public double AccuracyRate { get; set; }
    
    /// <summary>
    /// Média geral de tempo de resposta (ms) entre todas as emoções e tentativas
    /// </summary>
    public int AverageResponseTimeMs { get; set; }
    
    public Dictionary<string, Desafio2EmotionBreakdown> EmotionBreakdown { get; set; }
    public List<ProgressTrendItem> ProgressTrend { get; set; }
    
    /// <summary>
    /// Histórico detalhado de cada sessão com top predictions por emoção
    /// </summary>
    public List<Desafio2HistoricAttempt> HistoricAttempts { get; set; }
}

/// <summary>
/// Breakdown de métricas por emoção - Desafio 1 (sem confiança)
/// </summary>
public class Desafio1EmotionBreakdown
{
    public int Attempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double Accuracy { get; set; }
}

/// <summary>
/// Breakdown de métricas por emoção - Desafio 2 (com scores probabilísticos)
/// </summary>
public class Desafio2EmotionBreakdown
{
    public int Attempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double Accuracy { get; set; }
    
    /// <summary>
    /// Score médio da emoção alvo quando era o target
    /// </summary>
    public double AvgTargetScore { get; set; }
    
    /// <summary>
    /// Tempo médio de resposta em milissegundos para essa emoção
    /// </summary>
    public int AvgResponseTimeMs { get; set; }
}

/// <summary>
/// Histórico de uma sessão do Desafio 2
/// </summary>
public class Desafio2HistoricAttempt
{
    public DateTime Date { get; set; }
    public double AccuracyRate { get; set; }
    
    /// <summary>
    /// Detalhes por emoção nessa sessão (com top 3 predictions)
    /// </summary>
    public List<Desafio2AttemptDetail> Details { get; set; } = new();
}

/// <summary>
/// Detalhe de uma emoção específica dentro de uma sessão do Desafio 2
/// </summary>
public class Desafio2AttemptDetail
{
    public required string TargetEmotion { get; set; }
    public bool IsCorrect { get; set; }
    
    /// <summary>
    /// Tempo de resposta em milissegundos
    /// </summary>
    public int ResponseTimeMs { get; set; }
    
    /// <summary>
    /// Top 3 emoções detectadas com seus scores
    /// </summary>
    public List<Desafio2Prediction> TopPredictions { get; set; } = new();
}

/// <summary>
/// Predição de emoção com score (para métricas)
/// </summary>
public class Desafio2Prediction
{
    public required string Emotion { get; set; }
    public double Score { get; set; }
}

/// <summary>
/// Item de histórico de tentativa do Desafio 1
/// </summary>
public class Desafio1HistoricAttempt
{
    public DateTime Date { get; set; }
    public double AccuracyRate { get; set; }
    public string Level { get; set; } // "easy", "medium", "hard"
}

/// <summary>
/// Item de tendência de progresso por data
/// </summary>
public class ProgressTrendItem
{
    public DateTime Date { get; set; }
    public double Accuracy { get; set; }
}
