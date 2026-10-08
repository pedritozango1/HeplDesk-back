namespace Novati.API.Dtos.Dispositivos;

/// <summary>Pedido de atribuição de um dispositivo a um utilizador (ADMIN/TECNICO).</summary>
public class UpdateResponsavelDispositivoRequest
{
    /// <summary>Id do utilizador que passa a usar o dispositivo, ou null para o devolver à sala (partilhado).</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? ResponsavelId { get; set; }
}
