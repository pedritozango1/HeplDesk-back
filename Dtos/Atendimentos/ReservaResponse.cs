using Novati.API.Dtos.Compras;

namespace Novati.API.Dtos.Atendimentos;

/// <summary>
/// Resposta de <c>reservar-peca</c> quando existe stock: a peça ficou reservada para a ordem.
/// </summary>
public record ReservaOkResponse(bool Ok, OrdemDto Ordem, Guid UnidadeStockId, string Codigo);

/// <summary>
/// Resposta de <c>reservar-peca</c> quando não existe stock: foi criada uma requisição de compra.
/// O front usa <c>requisicao.itemStockId</c> e <c>requisicaoId</c>.
/// </summary>
public record ReservaFalhaResponse(bool Ok, OrdemDto Ordem, RequisicaoDto Requisicao, Guid RequisicaoId);
