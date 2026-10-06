using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Catalogo;

/// <summary>Pedido de criação de uma compatibilidade (só ADMIN).</summary>
public class CreateCompatibilidadeRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid ModeloDispositivoId { get; set; }

    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    [Required]
    public Guid ModeloComponenteId { get; set; }
}
