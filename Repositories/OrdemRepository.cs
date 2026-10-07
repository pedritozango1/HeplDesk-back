using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class OrdemRepository(AppDbContext ctx) : Repository<OrdemReparo>(ctx), IOrdemRepository
{
    public async Task<List<Guid>> GetSolicitacaoIdsPorTecnicoAsync(Guid tecnicoId, CancellationToken ct = default)
        => await Set.Where(o => o.TecnicoId == tecnicoId).Select(o => o.SolicitacaoId).ToListAsync(ct);

    public async Task<Guid?> GetTecnicoIdPorSolicitacaoAsync(Guid solicitacaoId, CancellationToken ct = default)
        => await Set.Where(o => o.SolicitacaoId == solicitacaoId).Select(o => (Guid?)o.TecnicoId).FirstOrDefaultAsync(ct);

    // O DTO precisa das coleções filhas — só o que for Include entra no resultado.
    private IQueryable<OrdemReparo> ComRelacoes(IQueryable<OrdemReparo> q) => q
        .Include(o => o.PecasUsadas)
        .Include(o => o.Rejeicoes)
        .Include(o => o.Historico).ThenInclude(h => h.Autor)
        .Include(o => o.Solicitacao);

    public async Task<OrdemReparo?> GetCompletaAsync(Guid id, CancellationToken ct = default)
        => await ComRelacoes(Set).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<OrdemReparo?> GetComHistoricoPorSolicitacaoAsync(Guid solicitacaoId, CancellationToken ct = default)
        => await Set.Include(o => o.Historico).FirstOrDefaultAsync(o => o.SolicitacaoId == solicitacaoId, ct);

    public async Task<List<OrdemReparo>> GetTodasCompletasAsync(IEnumerable<Guid>? solicitacaoIds = null, CancellationToken ct = default)
    {
        var q = ComRelacoes(Set);

        if (solicitacaoIds is not null)
        {
            var ids = solicitacaoIds.ToList();
            if (ids.Count == 0)
                return [];
            q = q.Where(o => ids.Contains(o.SolicitacaoId));
        }

        return await q.OrderBy(o => o.DataInicio).ToListAsync(ct);
    }

    public async Task<Guid?> GetRelatorioIdAsync(Guid ordemId, CancellationToken ct = default)
        => await Set.Where(o => o.Id == ordemId)
                    .Select(o => o.Relatorio != null ? (Guid?)o.Relatorio.Id : null)
                    .FirstOrDefaultAsync(ct);

    public async Task AddPecaAsync(OrdemPecaUsada peca, CancellationToken ct = default)
        => await ctx.Set<OrdemPecaUsada>().AddAsync(peca, ct);

    public async Task AddRejeicaoAsync(OrdemRejeicao rejeicao, CancellationToken ct = default)
        => await ctx.Set<OrdemRejeicao>().AddAsync(rejeicao, ct);
}
