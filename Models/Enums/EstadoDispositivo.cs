namespace Novati.API.Models.Enums;

/// <summary>
/// Estado operacional de um <c> DispositivoFisico</c> no inventário.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum EstadoDispositivo
{
    /// <summary>Em uso normal.</summary>
    ATIVO,

    /// <summary>Temporariamente fora de serviço, em intervenção técnica.</summary>
    MANUTENCAO,

    /// <summary>Fora de uso permanentemente (abatido/desativado).</summary>
    INATIVO,
}
