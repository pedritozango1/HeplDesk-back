using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Users;

namespace Novati.API.Services.Interfaces;
public interface IUserService
{
    Task<UserDto> GetMeAsync(Guid userId, CancellationToken ct = default);
    Task<List<UserDto>> GetDirectoryAsync(CancellationToken ct = default);
    Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<PaginaResultado<UserDto>> GetPaginaAsync(PaginacaoQuery paginacao, string? role, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
    /// <summary>O próprio utilizador altera o seu nome e email (nunca o perfil de acesso).</summary>
    Task<UserDto> UpdateMeAsync(Guid userId, UpdateMeRequest request, CancellationToken ct = default);

    /// <summary>O próprio utilizador troca a password: confirma a atual e a nova tem de ser forte.</summary>
    Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request, CancellationToken ct = default);

    Task<UserDto> UpdateSignatureAsync(Guid userId, SignatureRequest request, CancellationToken ct = default);
}