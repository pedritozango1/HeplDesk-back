using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Catalogo;

/// <summary>Pedido de criação de modelo de componente (só ADMIN).</summary>
public class CreateModeloComponenteRequest
{
    /// <example>RAM 8GB DDR4</example>
    [Required]
    public string Nome { get; set; } = "";

    /// <example>RAM</example>
    [Required]
    public string Tipo { get; set; } = "";

    /// <summary>Opcional no formulário do front — pode vir vazia.</summary>
    /// <example>8GB</example>
    public string Capacidade { get; set; } = "";

    /// <summary>Se omitido, assume 1.</summary>
    /// <example>3</example>
    public int? StockMinimo { get; set; }
}
