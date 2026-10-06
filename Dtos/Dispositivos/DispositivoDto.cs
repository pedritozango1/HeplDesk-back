namespace Novati.API.Dtos.Dispositivos;

/// <summary>Dispositivo físico no inventário.</summary>
public record DispositivoDto(
    Guid Id,
    string Patrimonio,
    Guid ModeloDispositivoId,
    string NumeroSerie,
    Guid LocalizacaoId,
    Guid? ResponsavelId,
    string Estado,
    DateOnly? DataAquisicao,
    int? GarantiaMeses
);
