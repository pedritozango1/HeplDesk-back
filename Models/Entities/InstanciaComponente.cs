using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;

/// <summary>Componente instalado num dispositivo físico.</summary>
public class InstanciaComponente : BaseEntity
{
    public string Codigo { get; set; } = "";
    public EstadoInstancia Estado { get; set; } = EstadoInstancia.INSTALADO;

    public Guid DispositivoFisicoId { get; set; }
    public DispositivoFisico DispositivoFisico { get; set; } = null!;

    public Guid ModeloComponenteId { get; set; }
    public ModeloComponente ModeloComponente { get; set; } = null!;
}