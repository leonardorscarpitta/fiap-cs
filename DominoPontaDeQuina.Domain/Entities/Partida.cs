namespace DominoPontaDeQuina.Domain.Entities;

public class Partida
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime IniciadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizadoEm { get; set; }
    public StatusJogo Status { get; set; } = StatusJogo.Aguardando;
    public ICollection<ParticipacaoPartida> Participacoes { get; set; } = new List<ParticipacaoPartida>();
}
