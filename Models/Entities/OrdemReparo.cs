using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;
/// <summary>Ordem de reparação associada a uma solicitação.</summary>
public class OrdemReparo : BaseEntity
{
    public string Diagnostico { get; set; } = "";
    public string? Solucao { get; set; }
    public string? SolucaoSugerida { get; set; }
    public EstadoOrdem Estado { get; set; } = EstadoOrdem.EM_DIAGNOSTICO;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }

    // ─── Cronómetro (sempre o relógio do servidor, em UTC — nunca um valor enviado pelo cliente) ───

    /// <summary>Momento em que o técnico assumiu a solicitação: arranque do cronómetro.</summary>
    public DateTime IniciadaEm { get; set; }

    /// <summary>Momento em que o solicitante aprovou a solução: paragem do cronómetro. Null nas ordens anteriores ao cronómetro.</summary>
    public DateTime? ConcluidaEm { get; set; }

    /// <summary>Momento da proposta que está à espera de resposta (null se não houver nenhuma pendente).</summary>
    public DateTime? PropostaEm { get; set; }

    /// <summary>Segundos acumulados à espera da resposta do solicitante (somam-se a cada aceitação/recusa).</summary>
    public int EsperaValidacaoSeg { get; set; }

    /// <summary>Minutos entre assumir e aprovar. Calculado na aprovação.</summary>
    public int? TempoGastoMin { get; set; }

    /// <summary>TempoGastoMin sem os períodos à espera de validação. Calculado na aprovação.</summary>
    public int? TempoTrabalhoMin { get; set; }

    // FK obrigatória 1–1 para Solicitacao
    public Guid SolicitacaoId { get; set; }
    public Solicitacao Solicitacao { get; set; } = null!;

    // FK obrigatória para User (técnico)
    public Guid TecnicoId { get; set; }
    public User Tecnico { get; set; } = null!;

    // Navegações inversas
    public List<OrdemPecaUsada> PecasUsadas { get; set; } = [];
    public List<OrdemRejeicao> Rejeicoes { get; set; } = [];
    public List<OrdemHistorico> Historico { get; set; } = [];
    public List<RequisicaoCompra> Requisicoes { get; set; } = [];

    // 1–0..1 com RelatorioTecnico
    public RelatorioTecnico? Relatorio { get; set; }
}
