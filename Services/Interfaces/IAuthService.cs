using Novati.API.Dtos.Auth;

namespace Novati.API.Services.Interfaces;
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
