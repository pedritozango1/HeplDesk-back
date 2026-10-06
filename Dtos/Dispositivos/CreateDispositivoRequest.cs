using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Dispositivos;

/// <summary>Pedido de criação de dispositivo físico (ADMIN/TECNICO).</summary>
public class CreateDispositivoRequest
{
    /// <example>NB-004</example>
    [Required]
    public string Patrimonio { get; set; } = "";

    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid ModeloDispositivoId { get; set; }

    /// <summary>Opcional no formulário do front — pode vir vazio.</summary>
    /// <example>PF-9Z8Y7X</example>
    public string NumeroSerie { get; set; } = "";

    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    [Required]
    public Guid LocalizacaoId { get; set; }

    /// <summary>Id do utilizador responsável, ou null.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? ResponsavelId { get; set; }

    /// <example>2024-03-01</example>
    public DateOnly? DataAquisicao { get; set; }

    /// <example>24</example>
    public int? GarantiaMeses { get; set; }
}
