using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;

/// <summary>Dispositivo físico no inventário (ex.: "NB-001").</summary>
public class DispositivoFisico : BaseEntity
{
    public string Patrimonio { get; set; } = "";
    public string NumeroSerie { get; set; } = "";
    public EstadoDispositivo Estado { get; set; } = EstadoDispositivo.ATIVO;
    public DateOnly? DataAquisicao { get; set; }
    public int? GarantiaMeses { get; set; }

    // FK obrigatória para ModeloDispositivo
    public Guid ModeloDispositivoId { get; set; }
    public ModeloDispositivo ModeloDispositivo { get; set; } = null!;

    // FK obrigatória para Localizacao
    public Guid LocalizacaoId { get; set; }
    public Localizacao Localizacao { get; set; } = null!;

    // FK opcional para User (responsável)
    public Guid? ResponsavelId { get; set; }
    public User? Responsavel { get; set; }

    // Navegação inversa
    public List<InstanciaComponente> Instancias { get; set; } = [];
}