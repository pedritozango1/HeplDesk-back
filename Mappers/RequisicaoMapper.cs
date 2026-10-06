using Novati.API.Dtos.Compras;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class RequisicaoMapper
{
    public static RequisicaoDto ToDto(RequisicaoCompra r) => new(
        r.Id,
        r.ItemStockId,
        r.ModeloComponenteId,
        r.Quantidade,
        r.Justificativa,
        r.SolicitanteId,
        r.OrdemId,
        r.Data,
        r.Estado.ToString());
}
