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
/// Dados de métricas para o Desafio 2 - Expressar emoções com confiança
/// </summary>
public class Desafio2MetricsData
{
    public int TotalSessions { get; set; }
    public double AccuracyRate { get; set; }
    public int AverageResponseTimeMs { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, Desafio2EmotionBreakdown> EmotionBreakdown { get; set; }
    public List<ProgressTrendItem> ProgressTrend { get; set; }
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
/// Breakdown de métricas por emoção - Desafio 2 (com confiança)
/// </summary>
public class Desafio2EmotionBreakdown
{
    public int Attempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double Accuracy { get; set; }
    public double AvgConfidence { get; set; }
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
