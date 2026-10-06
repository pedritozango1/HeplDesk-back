using Novati.API.Dtos.Stock;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

/// <summary>Converte as entidades de stock ⇄ DTO.</summary>
public static class StockMapper
{
    public static ItemStockDto ToDto(ItemStock i) => new(i.Id, i.ModeloComponenteId);

    public static UnidadeDto ToDto(UnidadeStock u) => new(u.Id, u.ItemStockId, u.Codigo, u.Estado.ToString(), u.ReservadaParaOrdemId);

    public static MovimentoDto ToDto(MovimentoStock m) => new(m.Id, m.ItemStockId, m.Tipo.ToString(), m.Quantidade, m.Data, m.Observacao);
}
