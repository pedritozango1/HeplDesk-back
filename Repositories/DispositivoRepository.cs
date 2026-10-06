using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class DispositivoRepository(AppDbContext ctx) : Repository<DispositivoFisico>(ctx), IDispositivoRepository
{
    public async Task<bool> PatrimonioExistsAsync(string patrimonio, CancellationToken ct = default)
        => await Set.AnyAsync(d => d.Patrimonio == patrimonio, ct);

    public async Task<PaginaResultado<DispositivoFisico>> GetPaginaAsync(PaginacaoQuery paginacao, EstadoDispositivo? estado, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        // As navegações (modelo, localização, responsável) viram JOINs no SQL — não é preciso Include
        // porque só servem para filtrar; o DTO só leva os Ids.
        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(d =>
                EF.Functions.ILike(d.Patrimonio, padrao) ||
                EF.Functions.ILike(d.ModeloDispositivo.Nome, padrao) ||
                EF.Functions.ILike(d.Localizacao.Nome, padrao) ||
                (d.Responsavel != null && EF.Functions.ILike(d.Responsavel.Nome, padrao)));

        if (estado is not null)
            q = q.Where(d => d.Estado == estado);

        return await q.OrderBy(d => d.Patrimonio).ThenBy(d => d.Id).ToPaginaAsync(paginacao, ct);
    }
}
