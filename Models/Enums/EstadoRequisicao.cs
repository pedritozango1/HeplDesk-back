namespace Novati.API.Models.Enums;

/// <summary>
/// Estado de uma <c>RequisicaoCompra</c>, criada quando falta stock de um componente.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum EstadoRequisicao
{
    /// <summary>Criada, à espera de decisão do gestor/admin.</summary>
    PENDENTE,

    /// <summary>Aprovada; compra autorizada.</summary>
    APROVADA,

    /// <summary>Rejeitada pelo gestor/admin.</summary>
    RECUSADA,

    /// <summary>Peças rececionadas e dadas entrada em stock.</summary>
    ENTREGUE,
}
