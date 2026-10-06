using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class ModeloComponenteRepository(AppDbContext ctx) : Repository<ModeloComponente>(ctx), IModeloComponenteRepository
{
    public async Task<PaginaResultado<ModeloComponente>> GetPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(m => EF.Functions.ILike(m.Nome, padrao) || EF.Functions.ILike(m.Capacidade, padrao));

        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(m => m.Tipo == tipo);

        return await q.OrderBy(m => m.Nome).ThenBy(m => m.Id).ToPaginaAsync(paginacao, ct);
    }
}
