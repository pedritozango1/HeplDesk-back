using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Settings;
using Novati.API.Dtos.Auth;
using Novati.API.Mappers;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Serviço de autenticação: valida credenciais e emite JWT.</summary>
public class AuthService(
    IUserRepository users,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // 1. Procurar utilizador pelo email
        var user = await users.GetByEmailAsync(request.Email, ct);

        // 2. Verificar password com BCrypt. Mesma mensagem para os dois casos (não revelar qual falhou).
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Credenciais inválidas.");

        // 3. Gerar o token
        var token = GerarToken(user.Id, user.Email, user.Role.ToString());

        return new LoginResponse(token, UserMapper.ToDto(user));
    }

    private string GerarToken(Guid userId, string email, string role)
    {
        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim("role", role),
            new Claim("email", email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}