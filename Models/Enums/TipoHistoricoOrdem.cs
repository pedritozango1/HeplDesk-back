namespace Novati.API.Models.Enums;

/// <summary>
/// Tipo de entrada no histórico (<c>OrdemHistorico</c>) de uma ordem de reparação — o registo
/// de auditoria de tudo o que acontece durante o atendimento.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum TipoHistoricoOrdem
{
    /// <summary>Técnico assumiu a ordem.</summary>
    ASSUMIDA,

    /// <summary>Diagnóstico registado.</summary>
    DIAGNOSTICO,

    /// <summary>Solução aplicada registada.</summary>
    SOLUCAO,

    /// <summary>Solicitante aceitou a solução proposta.</summary>
    ACEITE,

    /// <summary>Solicitante rejeitou a solução proposta.</summary>
    REJEITADA,

    /// <summary>Comentário livre adicionado ao histórico.</summary>
    COMENTARIO,

    /// <summary>Ordem reatribuída a outro técnico.</summary>
    REATRIBUIDA,
}
