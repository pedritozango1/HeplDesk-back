using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class ArtigoRepository(AppDbContext ctx) : Repository<Artigo>(ctx), IArtigoRepository
{
    public async Task<List<Artigo>> GetTodosComAvaliacoesAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().Include(a => a.Avaliacoes).ToListAsync(ct);

    public async Task<ArtigoAvaliacao?> GetAvaliacaoAsync(Guid artigoId, Guid userId, CancellationToken ct = default)
        => await ctx.Set<ArtigoAvaliacao>().FirstOrDefaultAsync(a => a.ArtigoId == artigoId && a.UserId == userId, ct);

    public async Task AddAvaliacaoAsync(ArtigoAvaliacao avaliacao, CancellationToken ct = default)
        => await ctx.Set<ArtigoAvaliacao>().AddAsync(avaliacao, ct);
}
