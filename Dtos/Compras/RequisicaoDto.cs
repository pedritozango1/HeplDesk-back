namespace Novati.API.Dtos.Compras;

/// <summary>Requisição de compra de peças, criada quando falta stock para uma reparação.</summary>
public record RequisicaoDto(
    Guid Id,
    Guid ItemStockId,
    Guid ModeloComponenteId,
    int Quantidade,
    string Justificativa,
    Guid SolicitanteId,
    Guid? OrdemId,
    DateOnly Data,
    string Estado
);
