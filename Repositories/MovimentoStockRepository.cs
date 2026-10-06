using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class MovimentoStockRepository(AppDbContext ctx) : Repository<MovimentoStock>(ctx), IMovimentoStockRepository
{
    public async Task<List<MovimentoStock>> GetAllOrderedAsync(CancellationToken ct = default)
        => await Set.OrderByDescending(m => m.Data).ToListAsync(ct);

    public async Task<PaginaResultado<MovimentoStock>> GetPaginaAsync(PaginacaoQuery paginacao, TipoMovimento? tipo, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(m =>
                EF.Functions.ILike(m.ItemStock.ModeloComponente.Nome, padrao) ||
                EF.Functions.ILike(m.Observacao, padrao));

        if (tipo is not null)
            q = q.Where(m => m.Tipo == tipo);

        // Mais recentes primeiro. Data só tem o dia — CriadoEm ordena dentro do mesmo dia.
        return await q
            .OrderByDescending(m => m.Data).ThenByDescending(m => m.CriadoEm).ThenBy(m => m.Id)
            .ToPaginaAsync(paginacao, ct);
    }
}
