using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;

/// <summary>Requisição de compra quando falta stock.</summary>
public class RequisicaoCompra : BaseEntity
{
    public int Quantidade { get; set; }
    public string Justificativa { get; set; } = "";
    public DateOnly Data { get; set; }

    // Instante exato da criação (UTC) — ordena as listagens paginadas dentro do mesmo dia.
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public EstadoRequisicao Estado { get; set; } = EstadoRequisicao.PENDENTE;

    public Guid ItemStockId { get; set; }
    public ItemStock ItemStock { get; set; } = null!;

    public Guid ModeloComponenteId { get; set; }
    public ModeloComponente ModeloComponente { get; set; } = null!;

    public Guid SolicitanteId { get; set; }
    public User Solicitante { get; set; } = null!;

    // Opcional: pode ter origem numa ordem
    public Guid? OrdemId { get; set; }
    public OrdemReparo? Ordem { get; set; }
}