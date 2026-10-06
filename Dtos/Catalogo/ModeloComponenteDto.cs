namespace Novati.API.Dtos.Catalogo;

/// <summary>Modelo de componente (ex.: "RAM 8GB DDR4").</summary>
public record ModeloComponenteDto(Guid Id, string Nome, string Tipo, string Capacidade, int StockMinimo);
