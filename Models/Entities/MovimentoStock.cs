using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;
/// <summary>Movimento de stock (entrada, saída, reserva, instalação).</summary>
public class MovimentoStock: BaseEntity
{
    public TipoMovimento Tipo { get; set; }
    public int Quantidade { get; set; }
    public DateOnly Data { get; set; }

    // Instante exato da criação (UTC) — ordena as listagens paginadas dentro do mesmo dia.
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string Observacao { get; set; } = string.Empty;

    public Guid ItemStockId { get; set; }
    public ItemStock ItemStock { get; set; } = null!;
}
