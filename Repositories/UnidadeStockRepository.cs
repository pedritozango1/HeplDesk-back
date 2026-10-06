using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class UnidadeStockRepository(AppDbContext ctx) : Repository<UnidadeStock>(ctx), IUnidadeStockRepository
{
    public async Task<bool> ExistsCodigoAsync(string codigo, CancellationToken ct = default)
        => await Set.AnyAsync(u => u.Codigo == codigo, ct);

    public async Task<UnidadeStock?> GetDisponivelAsync(Guid itemStockId, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(u => u.ItemStockId == itemStockId && u.Estado == EstadoUnidade.DISPONIVEL, ct);

    public async Task<List<string>> GetCodigosComPrefixoAsync(string prefixo, CancellationToken ct = default)
        => await Set.Where(u => u.Codigo.StartsWith(prefixo + "-")).Select(u => u.Codigo).ToListAsync(ct);

    public async Task<PaginaResultado<UnidadeStock>> GetPaginaAsync(PaginacaoQuery paginacao, EstadoUnidade? estado, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(u =>
                EF.Functions.ILike(u.Codigo, padrao) ||
                EF.Functions.ILike(u.ItemStock.ModeloComponente.Nome, padrao));

        if (estado is not null)
            q = q.Where(u => u.Estado == estado);

        return await q.OrderBy(u => u.Codigo).ThenBy(u => u.Id).ToPaginaAsync(paginacao, ct);
    }
}
