using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Paginacao;

namespace Novati.API.Common.Extensions;

/// <summary>Extensões de IQueryable para paginação e pesquisa.</summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Executa a query paginada: 1 COUNT (total com os filtros) + 1 SELECT com OFFSET/LIMIT.
    /// A query TEM de vir ordenada (OrderBy) e com um desempate único (ex.: ThenBy(Id)) —
    /// sem ordem estável, a BD pode repetir ou saltar registos entre páginas.
    /// </summary>
    public static async Task<PaginaResultado<T>> ToPaginaAsync<T>(
        this IQueryable<T> query, PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);

        var itens = await query
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync(ct);

        return new PaginaResultado<T>(itens, total, paginacao.Pagina, paginacao.TamanhoPagina);
    }

    /// <summary>
    /// Padrão "%texto%" para EF.Functions.ILike (contém, sem distinguir maiúsculas), ou null se não
    /// houver pesquisa. Escapa % e _ para o utilizador os poder procurar como texto normal.
    /// </summary>
    public static string? PadraoContem(string? pesquisa)
    {
        if (string.IsNullOrWhiteSpace(pesquisa))
            return null;

        var texto = pesquisa.Trim()
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");

        return $"%{texto}%";
    }
}
