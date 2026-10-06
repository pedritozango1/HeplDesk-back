namespace Novati.API.Models.Entities;
/// <summary>Rejeição de solução pelo solicitante.</summary>
public class OrdemRejeicao : BaseEntity
{
 public string Motivo { get; set; } = "";
    public DateOnly Data { get; set; }

    public Guid OrdemId { get; set; }
    public OrdemReparo Ordem { get; set; } = null!;   
}
