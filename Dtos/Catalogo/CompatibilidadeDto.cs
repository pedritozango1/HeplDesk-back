namespace Novati.API.Dtos.Catalogo;

/// <summary>Compatibilidade entre um modelo de dispositivo e um modelo de componente.</summary>
public record CompatibilidadeDto(Guid Id, Guid ModeloDispositivoId, Guid ModeloComponenteId);
