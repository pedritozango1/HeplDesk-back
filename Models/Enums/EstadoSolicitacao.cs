namespace Novati.API.Models.Enums;

/// <summary>
/// Estado de uma solicitação ao longo do seu ciclo de vida.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum EstadoSolicitacao
{
    /// <summary>Criada, ainda sem técnico atribuído.</summary>
    ABERTA,

    /// <summary>Um técnico assumiu a solicitação; existe uma <c>OrdemReparo</c> associada.</summary>
    EM_ATENDIMENTO,

    /// <summary>O técnico propôs solução; aguarda validação do solicitante.</summary>
    AGUARDA_VALIDACAO,

    /// <summary>O solicitante validou a solução.</summary>
    RESOLVIDA,

    /// <summary>Ciclo terminado, sem mais ações possíveis.</summary>
    FECHADA,
}
