using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Stock;

/// <summary>Pedido de entrada manual de stock (ADMIN/TECNICO). Cria N unidades + 1 movimento.</summary>
public class EntradaStockRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid ModeloComponenteId { get; set; }

    /// <example>5</example>
    [Required, Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    /// <summary>Se omitido, assume "Entrada manual".</summary>
    /// <example>Compra ao fornecedor X</example>
    public string? Observacao { get; set; }
}
