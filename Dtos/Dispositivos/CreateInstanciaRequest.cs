using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Dispositivos;

/// <summary>Pedido de criação de instância de componente num dispositivo (ADMIN/TECNICO).</summary>
public class CreateInstanciaRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid ModeloComponenteId { get; set; }

    /// <example>SSD-100</example>
    [Required]
    public string Codigo { get; set; } = "";
}
