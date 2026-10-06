namespace Novati.API.Models.Enums;

/// <summary>
/// Estado de um <c>RelatorioTecnico</c> ao longo do seu ciclo de revisão.
/// </summary>
/// <remarks>
/// Os membros ficam propositadamente em <c>minúsculas</c> — ao contrário dos restantes enums do
/// projeto — porque é assim que o front-end escreve e compara este valor. Sem isto, o
/// <c>JsonStringEnumConverter</c> geraria/exigiria texto em maiúsculas e a comparação no front falharia.
/// </remarks>
public enum StatusRelatorio
{
    /// <summary>Em edição pelo técnico autor, ainda não submetido.</summary>
    rascunho,

    /// <summary>Submetido pelo técnico, à espera de aprovação.</summary>
    finalizado,

    /// <summary>Aprovado por um gestor/admin; já não pode ser editado.</summary>
    aprovado,
}
