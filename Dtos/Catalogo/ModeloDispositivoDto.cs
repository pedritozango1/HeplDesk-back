namespace Novati.API.Dtos.Catalogo;

/// <summary>Modelo de dispositivo (ex.: "Lenovo ThinkPad E14").</summary>
public record ModeloDispositivoDto(Guid Id, string Nome, string Fabricante, string Tipo);
