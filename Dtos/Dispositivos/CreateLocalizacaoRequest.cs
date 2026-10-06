using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Dispositivos;

/// <summary>Pedido de criação de localização (ADMIN/TECNICO).</summary>
public class CreateLocalizacaoRequest
{
    /// <example>Sala 2</example>
    [Required]
    public string Nome { get; set; } = "";

    /// <summary>Id da localização-pai, ou null para uma raiz da árvore.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? Pai { get; set; }
}
