using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Dispositivos;

/// <summary>Pedido de mudança de estado de um dispositivo (ADMIN/TECNICO).</summary>
public class UpdateEstadoDispositivoRequest
{
    /// <summary>ATIVO, MANUTENCAO ou INATIVO.</summary>
    /// <example>MANUTENCAO</example>
    [Required]
    public string Estado { get; set; } = "";
}
