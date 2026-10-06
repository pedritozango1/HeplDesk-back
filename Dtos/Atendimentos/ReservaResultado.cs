using Novati.API.Dtos.Atendimentos;
using Novati.API.Dtos.Compras;

namespace Novati.API.Dtos.Atendimentos;

/// <summary>
/// Resultado de <c>reservar-peca</c> em qualquer um dos dois caminhos:
/// <c>Ok = true</c> traz <c>UnidadeStockId</c>/<c>Codigo</c>; <c>Ok = false</c> traz <c>Requisicao</c>.
/// </summary>
public record ReservaResultado(
    bool Ok,
    OrdemDto Ordem,
    Guid UnidadeStockId,
    string Codigo,
    RequisicaoDto? Requisicao
);
