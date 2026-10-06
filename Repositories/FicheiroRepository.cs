using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class FicheiroRepository(AppDbContext ctx) : Repository<Ficheiro>(ctx), IFicheiroRepository
{
    public async Task<List<Ficheiro>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var lista = ids.ToList();
        return await Set.Where(f => lista.Contains(f.Id)).ToListAsync(ct);
    }

    public async Task<bool> EstaEmUsoAsync(Guid id, CancellationToken ct = default)
        => await ctx.Users.AnyAsync(u => u.AssinaturaFicheiroId == id, ct)
        || await ctx.RelatoriosTecnicos.AnyAsync(r => r.AssinaturaTecnicoFicheiroId == id, ct)
        // Anexos vivem numa coluna JSONB (sem FK): a verificação tem de ser feita à mão.
        || await ctx.Solicitacoes.AnyAsync(s => s.Anexos.Any(a => a.FicheiroId == id), ct);
}
