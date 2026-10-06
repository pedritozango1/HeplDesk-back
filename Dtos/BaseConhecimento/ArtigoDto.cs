namespace Novati.API.Dtos.BaseConhecimento;

/// <summary>Artigo da base de conhecimento.</summary>
/// <summary>Artigo da base de conhecimento.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Titulo">Título.</param>
/// <param name="Conteudo">Texto (passos).</param>
/// <param name="Tags">Palavras-chave — também usadas nas perguntas de afinação do assistente.</param>
/// <param name="Categoria">Categoria (ver GET /api/dominio).</param>
/// <param name="AutorId">Quem escreveu.</param>
/// <param name="Uteis">Quantas pessoas responderam "Isto ajudou? Sim".</param>
/// <param name="NaoUteis">Quantas pessoas responderam "Não".</param>
public record ArtigoDto(Guid Id, string Titulo, string Conteudo, List<string> Tags, string Categoria, Guid AutorId, int Uteis, int NaoUteis);

/// <summary>Resposta a "Isto ajudou?" sobre um artigo.</summary>
public class AvaliarArtigoRequest
{
    public bool Util { get; set; }
}
