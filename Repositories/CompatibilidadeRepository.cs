using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class CompatibilidadeRepository(AppDbContext ctx) : Repository<Compatibilidade>(ctx), ICompatibilidadeRepository
{
    public async Task<bool> ExistsPairAsync(Guid modeloDispositivoId, Guid modeloComponenteId, CancellationToken ct = default)
        => await Set.AnyAsync(c =>
            c.ModeloDispositivoId == modeloDispositivoId &&
            c.ModeloComponenteId == modeloComponenteId, ct);

    public async Task<bool> ExisteAlgumaAsync(Guid modeloComponenteId, CancellationToken ct = default)
        => await Set.AnyAsync(c => c.ModeloComponenteId == modeloComponenteId, ct);
}
