namespace Novati.API.Models.Entities;
/// <summary>Relação N:N entre ModeloDispositivo e ModeloComponente.</summary>
public class Compatibilidade: BaseEntity
{
    public Guid ModeloDispositivoId { get; set; }
    public ModeloDispositivo ModeloDispositivo { get; set; } = null!;

    public Guid ModeloComponenteId { get; set; }
    public ModeloComponente ModeloComponente { get; set; } = null!;  
}
