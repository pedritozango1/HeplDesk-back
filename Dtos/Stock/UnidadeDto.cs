namespace Novati.API.Dtos.Stock;

/// <summary>Unidade física individual de stock.</summary>
public record UnidadeDto(Guid Id, Guid ItemStockId, string Codigo, string Estado, Guid? ReservadaParaOrdemId);
