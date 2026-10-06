using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class Repository<T>(AppDbContext ctx) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext ctx = ctx;
    protected DbSet<T> Set => ctx.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Set.FindAsync([id], ct);

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    // Entidade já seguida (carregada neste pedido): o change tracker deteta sozinho as
    // alterações e os filhos novos (INSERT). Set.Update() percorreria o grafo e marcaria
    // os filhos novos como Modified (UPDATE de 0 linhas → DbUpdateConcurrencyException).
    public void Update(T entity)
    {
        if (ctx.Entry(entity).State == EntityState.Detached)
            Set.Update(entity);
    }

    public void Remove(T entity) => Set.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await Set.AnyAsync(e => e.Id == id, ct);
}
