namespace Novati.API.Models.Enums;

/// <summary>
/// Estado da ordem de reparação (máquina de estados do atendimento técnico).
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum EstadoOrdem
{
    /// <summary>Técnico a investigar a causa do problema.</summary>
    EM_DIAGNOSTICO,

    /// <summary>Diagnóstico feito; reparação em curso (pode envolver reserva/instalação de peças).</summary>
    EM_REPARACAO,

    /// <summary>Solução proposta pelo técnico; aguarda validação do solicitante.</summary>
    AGUARDA_VALIDACAO,

    /// <summary>Solução aceite pelo solicitante; ordem concluída.</summary>
    RESOLVIDO,
}
