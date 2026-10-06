namespace Novati.API.Models.Entities;
/// <summary>
/// Peça usada/reservada numa ordem de reparação.
/// </summary>
public class OrdemPecaUsada : BaseEntity
{
    public Guid OrdemId { get; set; }
    public OrdemReparo Ordem { get; set; } = null!;

    public Guid UnidadeStockId { get; set; }
    public UnidadeStock UnidadeStock { get; set; } = null!;

    public Guid ModeloComponenteId { get; set; }
    public ModeloComponente ModeloComponente { get; set; } = null!;

    // Se a peça já foi instalada, aponta para a instância criada
    public Guid? InstaladoInstanciaId { get; set; }
    public InstanciaComponente? InstaladoInstancia { get; set; }
}
