using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>Página de utilizadores por nome. Pesquisa em nome e email; filtro opcional por perfil.</summary>
    Task<PaginaResultado<User>> GetPaginaAsync(PaginacaoQuery paginacao, Role? role, CancellationToken ct = default);
}
