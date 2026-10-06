using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

/// <summary>Implementação específica de UserRepository.</summary>
public class UserRepository(AppDbContext ctx) : Repository<User>(ctx), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
        => await Set.AnyAsync(u => u.Email == email && (excludeId == null || u.Id != excludeId), ct);

    public async Task<PaginaResultado<User>> GetPaginaAsync(PaginacaoQuery paginacao, Role? role, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (QueryableExtensions.PadraoContem(paginacao.Pesquisa) is { } padrao)
            q = q.Where(u => EF.Functions.ILike(u.Nome, padrao) || EF.Functions.ILike(u.Email, padrao));

        if (role is not null)
            q = q.Where(u => u.Role == role);

        return await q.OrderBy(u => u.Nome).ThenBy(u => u.Id).ToPaginaAsync(paginacao, ct);
    }
}
