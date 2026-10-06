using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;
/// <summary>Unidade física individual de stock (ex.: "RAM-050").</summary>
public class UnidadeStock: BaseEntity
{
    public string Codigo { get; set; } = "";
    public EstadoUnidade Estado { get; set; } = EstadoUnidade.DISPONIVEL;

    public Guid ItemStockId { get; set; }
    public ItemStock ItemStock { get; set; } = null!;

    // Reserva opcional para uma ordem de reparação
    public Guid? ReservadaParaOrdemId { get; set; }
    public OrdemReparo? ReservadaParaOrdem { get; set; }
}
