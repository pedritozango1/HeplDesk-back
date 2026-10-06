using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class RelatorioRepository(AppDbContext ctx) : Repository<RelatorioTecnico>(ctx), IRelatorioRepository
{
    public async Task<List<RelatorioTecnico>> GetVisiveisAsync(
        StatusRelatorio[]? estados = null,
        IEnumerable<Guid>? solicitacaoIds = null,
        CancellationToken ct = default)
    {
        // A ordem entra no Include porque a visibilidade do FUNCIONARIO passa pelas suas solicitações.
        IQueryable<RelatorioTecnico> q = Set.Include(r => r.Historico).Include(r => r.Ordem);

        if (estados is not null)
        {
            if (estados.Length == 0)
                return [];
            q = q.Where(r => estados.Contains(r.Status));
        }

        if (solicitacaoIds is not null)
        {
            var ids = solicitacaoIds.ToList();
            if (ids.Count == 0)
                return [];
            q = q.Where(r => ids.Contains(r.Ordem.SolicitacaoId));
        }

        return await q.OrderBy(r => r.CriadoEm).ToListAsync(ct);
    }

    public async Task<RelatorioTecnico?> GetComHistoricoAsync(Guid id, CancellationToken ct = default)
        => await Set.Include(r => r.Historico).Include(r => r.Ordem).FirstOrDefaultAsync(r => r.Id == id, ct);
}
