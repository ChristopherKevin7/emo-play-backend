namespace EMOPlay.Application.DTOs.Game;

public class SessionResultRequest
{
    public Guid SessionId { get; set; }
    public int Acertos { get; set; }
    public double Percentage { get; set; }
    public required string Mensagem { get; set; }
    public required List<ResultadoItem> Resultados { get; set; }
}

public class ResultadoItem
{
    public required string Emoção { get; set; }
    public required string Status { get; set; } // "correct" ou "incorrect"
}
