using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class ItemStockRepository(AppDbContext ctx) : Repository<ItemStock>(ctx), IItemStockRepository
{
    public async Task<ItemStock> GetOrCreateByModeloAsync(Guid modeloComponenteId, CancellationToken ct = default)
    {
        var existente = await Set.FirstOrDefaultAsync(i => i.ModeloComponenteId == modeloComponenteId, ct);
        if (existente is not null)
            return existente;

        var novo = new ItemStock { ModeloComponenteId = modeloComponenteId };
        await Set.AddAsync(novo, ct);
        return novo;
    }
}
