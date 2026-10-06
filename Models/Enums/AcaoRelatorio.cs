namespace Novati.API.Models.Enums;

/// <summary>
/// Ação registada no histórico (<c>RelatorioHistorico</c>) de um relatório técnico.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum AcaoRelatorio
{
    /// <summary>Relatório criado.</summary>
    CRIADO,

    /// <summary>Campo do relatório editado.</summary>
    EDITADO,

    /// <summary>Relatório submetido para aprovação.</summary>
    FINALIZADO,

    /// <summary>Relatório reaberto para nova edição.</summary>
    REABERTO,

    /// <summary>Relatório aprovado por um gestor/admin.</summary>
    APROVADO,
}
