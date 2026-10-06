namespace Novati.API.Models.Entities;

/// <summary>Mensagem de chat associada a uma solicitação.</summary>
public class Mensagem : BaseEntity
{
    public string Texto { get; set; } = "";
    public DateOnly Data { get; set; }
    public TimeOnly Hora { get; set; }
    public bool Lida { get; set; }

    public Guid SolicitacaoId { get; set; }
    public Solicitacao Solicitacao { get; set; } = null!;

    public Guid AutorId { get; set; }
    public User Autor { get; set; } = null!;
}