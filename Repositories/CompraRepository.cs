using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class CompraRepository(AppDbContext ctx) : Repository<RequisicaoCompra>(ctx), ICompraRepository
{
    public async Task<List<RequisicaoCompra>> GetAllOrderedAsync(CancellationToken ct = default)
        => await Set.OrderBy(r => r.Data).ToListAsync(ct);

    public async Task<PaginaResultado<RequisicaoCompra>> GetPaginaAsync(PaginacaoQuery paginacao, EstadoRequisicao? estado, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(r =>
                EF.Functions.ILike(r.ModeloComponente.Nome, padrao) ||
                EF.Functions.ILike(r.Justificativa, padrao) ||
                EF.Functions.ILike(r.Solicitante.Nome, padrao));

        if (estado is not null)
            q = q.Where(r => r.Estado == estado);

        // Mais recentes primeiro. Data só tem o dia — CriadoEm ordena dentro do mesmo dia.
        return await q
            .OrderByDescending(r => r.Data).ThenByDescending(r => r.CriadoEm).ThenBy(r => r.Id)
            .ToPaginaAsync(paginacao, ct);
    }
}
