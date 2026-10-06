using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;
/// <summary>Pedido de suporte aberto por um funcionário.</summary>
public class Solicitacao: BaseEntity
{
   public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public EstadoSolicitacao Estado { get; set; } = EstadoSolicitacao.ABERTA;
    public Prioridade Prioridade { get; set; }
    public string Categoria { get; set; } = "";
    public DateOnly DataCriacao { get; set; }

    // Instante exato da criação (UTC). DataCriacao só guarda o dia; isto ordena as listagens
    // paginadas dentro do mesmo dia.
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public bool ResolvidaViaBase { get; set; }

    // FK obrigatória para User (solicitante)
    public Guid SolicitanteId { get; set; }
    public User Solicitante { get; set; } = null!;

    // FK opcional para DispositivoFisico
    public Guid? DispositivoFisicoId { get; set; }
    public DispositivoFisico? DispositivoFisico { get; set; }

    // Lista JSONB
    public List<Anexo> Anexos { get; set; } = [];

    // Owned type opcional
    public Avaliacao? Avaliacao { get; set; }

    // Navegação inversa 1–1 (a OrdemReparo tem SolicitacaoId)
    public OrdemReparo? Ordem { get; set; }

    // Navegação inversa 1:N
    public List<Mensagem> Mensagens { get; set; } = []; 
}
