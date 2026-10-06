namespace Novati.API.Dtos.Stock;

/// <summary>Item de stock — 1 por modelo de componente.</summary>
public record ItemStockDto(Guid Id, Guid ModeloComponenteId);
