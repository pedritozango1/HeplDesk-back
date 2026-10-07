using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;

/// <summary>
/// Pedido de acesso remoto do técnico ao PC do solicitante, e a sessão que dele resulta.
/// Guarda quem pediu, quem autorizou, quando e porquê terminou — nunca o que se viu no ecrã.
/// </summary>
public class SessaoRemota : BaseEntity
{
    public EstadoSessaoRemota Estado { get; set; } = EstadoSessaoRemota.PEDIDA;
    public ModoAcessoRemoto Modo { get; set; } = ModoAcessoRemoto.VER;

    // ─── Token de uso único (sempre o relógio do servidor, em UTC) ───

    /// <summary>SHA-256 do token. O token em claro nunca é guardado. Null antes de autorizar e depois de usado.</summary>
    public string? TokenHash { get; set; }
    public DateTime? TokenExpiraEm { get; set; }

    /// <summary>Momento do resgate. Preenchido uma única vez: é o que torna o token de uso único.</summary>
    public DateTime? TokenUsadoEm { get; set; }

    /// <summary>Resgates com o código errado. Ao chegar ao máximo a sessão expira.</summary>
    public int TentativasFalhadas { get; set; }

    // ─── Ciclo de vida ───

    public DateTime PedidaEm { get; set; } = DateTime.UtcNow;

    /// <summary>Momento em que o solicitante autorizou ou recusou.</summary>
    public DateTime? RespondidaEm { get; set; }
    public DateTime? IniciadaEm { get; set; }
    public DateTime? TerminadaEm { get; set; }

    /// <summary>Quem terminou. Null quando foi o sistema (expiração, tempo máximo).</summary>
    public Guid? TerminadaPorId { get; set; }
    public string? MotivoFim { get; set; }

    /// <summary>Endereço de onde veio a autorização: prova do consentimento.</summary>
    public string? AutorizadaIp { get; set; }

    // ─── Modo CONTROLAR (Agente Novati) ───

    /// <summary>Nome do PC onde corre o agente associado pelo solicitante ao autorizar. Fica para auditoria.</summary>
    public string? AgentePc { get; set; }

    /// <summary>Versão da linha (xmin do PostgreSQL): impede duas gravações simultâneas sobre a mesma sessão.</summary>
    public uint Versao { get; set; }

    // FK obrigatória para Solicitacao
    public Guid SolicitacaoId { get; set; }
    public Solicitacao Solicitacao { get; set; } = null!;

    // FK obrigatória para User (técnico que pediu — só ele pode resgatar o token)
    public Guid TecnicoId { get; set; }
    public User Tecnico { get; set; } = null!;

    // FK obrigatória para User (solicitante — só ele pode autorizar)
    public Guid SolicitanteId { get; set; }
    public User Solicitante { get; set; } = null!;

    /// <summary>Ainda não chegou a um estado final.</summary>
    public bool EmCurso => Estado is EstadoSessaoRemota.PEDIDA or EstadoSessaoRemota.AUTORIZADA or EstadoSessaoRemota.ATIVA;
}
