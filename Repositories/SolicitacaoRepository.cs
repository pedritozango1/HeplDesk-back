using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class SolicitacaoRepository(AppDbContext ctx) : Repository<Solicitacao>(ctx), ISolicitacaoRepository
{
    public async Task<List<Solicitacao>> GetBySolicitanteAsync(Guid solicitanteId, CancellationToken ct = default)
        => await Set.Where(s => s.SolicitanteId == solicitanteId).ToListAsync(ct);

    public async Task<PaginaResultado<Solicitacao>> GetPaginaAsync(
        PaginacaoQuery paginacao, EstadoSolicitacao? estado, Guid? solicitanteId, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (solicitanteId is not null)
            q = q.Where(s => s.SolicitanteId == solicitanteId);

        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(s => EF.Functions.ILike(s.Titulo, padrao) || EF.Functions.ILike(s.Descricao, padrao));

        if (estado is not null)
            q = q.Where(s => s.Estado == estado);

        // Mais recentes primeiro. DataCriacao só tem o dia — CriadoEm ordena dentro do mesmo dia.
        return await q
            .OrderByDescending(s => s.DataCriacao).ThenByDescending(s => s.CriadoEm).ThenBy(s => s.Id)
            .ToPaginaAsync(paginacao, ct);
    }
}
