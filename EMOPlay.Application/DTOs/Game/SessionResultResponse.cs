namespace EMOPlay.Application.DTOs.Game;

public class SessionResultResponse
{
    public Guid SessionId { get; set; }
    public int Acertos { get; set; }
    public double Percentage { get; set; }
    public int Score { get; set; }
    public required string Mensagem { get; set; }
    public required List<ResultadoItem> Resultados { get; set; }
    public bool Armazenado { get; set; }
    public required string Mensagem_Retorno { get; set; }
}
