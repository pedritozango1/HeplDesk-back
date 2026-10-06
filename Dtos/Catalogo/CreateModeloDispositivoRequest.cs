using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Catalogo;

/// <summary>Pedido de criação de modelo de dispositivo (só ADMIN).</summary>
public class CreateModeloDispositivoRequest
{
    /// <example>Lenovo ThinkPad E14</example>
    [Required]
    public string Nome { get; set; } = "";

    /// <example>Lenovo</example>
    [Required]
    public string Fabricante { get; set; } = "";

    /// <example>Laptop</example>
    [Required]
    public string Tipo { get; set; } = "";
}
