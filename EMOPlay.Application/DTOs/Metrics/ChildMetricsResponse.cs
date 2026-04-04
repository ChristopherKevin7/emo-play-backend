namespace EMOPlay.Application.DTOs.Metrics;

public class ChildMetricsResponse
{
    public Guid ChildId { get; set; }
    public string ChildName { get; set; }
    public int TotalSessions { get; set; }
    public double AccuracyRate { get; set; }
    public int AverageResponseTimeMs { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, EmotionMetricsBreakdown> EmotionBreakdown { get; set; }
    public List<ProgressTrendItem> ProgressTrend { get; set; }
    public string Disclaimer { get; set; }
}

public class EmotionMetricsBreakdown
{
    public int Attempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double Accuracy { get; set; }
    public double AvgConfidence { get; set; }
}

public class ProgressTrendItem
{
    public DateTime Date { get; set; }
    public double Accuracy { get; set; }
}
