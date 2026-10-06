using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class NotificacaoRepository(AppDbContext ctx) : Repository<Notificacao>(ctx), INotificacaoRepository
{
    public async Task<List<Notificacao>> GetByUserOrderedAsync(Guid userId, CancellationToken ct = default)
        => await Set.Where(n => n.UserId == userId).OrderBy(n => n.Data).ToListAsync(ct);
}
