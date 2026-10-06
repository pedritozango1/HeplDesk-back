using System.ComponentModel.DataAnnotations;
using Novati.API.Dtos.BaseConhecimento;

namespace Novati.API.Dtos.Assistente;

/// <summary>Uma pergunta de afinação já respondida pelo utilizador.</summary>
public class RespostaAssistenteDto
{
    [Required] public string Pergunta { get; set; } = "";
    [Required] public string Resposta { get; set; } = "";

    /// <summary>Opções que foram mostradas — o motor usa-as para "Nenhuma destas" e para não repetir perguntas.</summary>
    public List<string> Opcoes { get; set; } = [];
}

/// <summary>
/// Pedido de análise. Indica-se a solicitação (funcionário) ou a ordem (técnico);
/// `Respostas` são as perguntas de afinação já respondidas nesta sessão.
/// </summary>
public class AnalisarRequest
{
    public Guid? SolicitacaoId { get; set; }
    public Guid? OrdemId { get; set; }
    public List<RespostaAssistenteDto> Respostas { get; set; } = [];

    /// <summary>"Ver solução agora": não fazer mais perguntas.</summary>
    public bool ForcarPlano { get; set; }
}

public record PerguntaAssistenteDto(string Texto, List<string> Opcoes);

public record PlanoAssistenteDto(
    string Resumo,
    List<string> Causas,
    List<string> Passos,
    string Solucao,
    List<ArtigoDto> Artigos);

/// <summary>
/// Resposta do assistente: ou mais uma pergunta de afinação, ou o plano final.
/// </summary>
/// <param name="Tipo">"pergunta" ou "plano".</param>
/// <param name="Origem">"motor": motor de sugestões do backend, calculado com os artigos, as avaliações e as reparações resolvidas da BD.</param>
/// <param name="Pergunta">Preenchido quando Tipo = "pergunta".</param>
/// <param name="Plano">Preenchido quando Tipo = "plano".</param>
public record AssistenteRespostaDto(string Tipo, string Origem, PerguntaAssistenteDto? Pergunta, PlanoAssistenteDto? Plano);
