namespace Novati.API.Dtos.Stock;

/// <summary>Movimento de stock (entrada, saída, reserva, instalação, transferência).</summary>
public record MovimentoDto(Guid Id, Guid ItemStockId, string Tipo, int Quantidade, DateOnly Data, string Observacao);
