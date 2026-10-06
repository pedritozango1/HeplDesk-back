namespace Novati.API.Common.Paginacao;

/// <summary>
/// Uma página de resultados + o total de registos que cumprem os filtros
/// (o front precisa do total para desenhar "página 2 de 7").
/// </summary>
public record PaginaResultado<T>(IReadOnlyList<T> Itens, int Total, int Pagina, int TamanhoPagina)
{
    /// <summary>Número de páginas (0 quando não há registos).</summary>
    public int TotalPaginas => (int)Math.Ceiling(Total / (double)TamanhoPagina);

    /// <summary>Página sem registos (ex.: perfil sem acesso à listagem — filtra em vez de devolver 403).</summary>
    public static PaginaResultado<T> Vazia(PaginacaoQuery paginacao)
        => new([], 0, paginacao.Pagina, paginacao.TamanhoPagina);

    /// <summary>Converte os itens (entidade → DTO) mantendo os números da paginação.</summary>
    public PaginaResultado<TOut> Map<TOut>(Func<T, TOut> mapper)
        => new(Itens.Select(mapper).ToList(), Total, Pagina, TamanhoPagina);
}
