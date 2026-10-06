namespace Novati.API.Models.Entities;
/// <summary>
/// Item de stock — 1 por ModeloComponente.
/// Representa "o tipo de peça" (ex.: "RAM 8GB DDR4").
/// </summary>
public class ItemStock : BaseEntity
{
    public Guid ModeloComponenteId { get; set; }
    public ModeloComponente ModeloComponente { get; set; } = null!;

    // Navegações inversas
    public List<UnidadeStock> Unidades { get; set; } = [];
    public List<MovimentoStock> Movimentos { get; set; } = [];
    public List<RequisicaoCompra> Requisicoes { get; set; } = [];  
}
