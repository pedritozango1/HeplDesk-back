using System.ComponentModel.DataAnnotations;

namespace Novati.API.Common.Paginacao;

/// <summary>
/// Parâmetros de paginação lidos da query string (?pagina=2&amp;tamanhoPagina=25&amp;pesquisa=dell).
/// Os limites são validados pelo [ApiController] antes de o pedido chegar ao controller (HTTP 400).
/// </summary>
public class PaginacaoQuery
{
    /// <summary>Teto do tamanho de página: impede que um cliente peça a tabela inteira de uma vez.</summary>
    public const int TamanhoMaximo = 100;

    /// <summary>Página pedida, a começar em 1.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "A página tem de ser 1 ou superior.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantos registos por página (1 a 100).</summary>
    [Range(1, TamanhoMaximo, ErrorMessage = "O tamanho da página tem de estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;

    /// <summary>Texto livre a procurar (cada listagem decide em que colunas).</summary>
    public string? Pesquisa { get; set; }
}
