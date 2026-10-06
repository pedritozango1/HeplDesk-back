using System.Security.Claims;
using Novati.API.Models.Enums;

namespace Novati.API.Common.Extensions;

/// <summary>Extensões para ler claims do utilizador autenticado.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Devolve o Id do utilizador autenticado (claim "sub").</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value
                  ?? throw new InvalidOperationException("Claim 'sub' não encontrada.");
        return Guid.Parse(sub);
    }

    /// <summary>Devolve a role do utilizador autenticado (claim "role").</summary>
    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst("role")?.Value
               ?? throw new InvalidOperationException("Claim 'role' não encontrada.");
    }

    /// <summary>Devolve a role do utilizador autenticado já convertida para o enum <see cref="Role"/>.</summary>
    public static Role GetRoleAsEnum(this ClaimsPrincipal user)
    {
        var role = user.GetRole();
        if (!Enum.TryParse<Role>(role, out var resultado))
            throw new InvalidOperationException($"Role inválida no token: '{role}'.");

        return resultado;
    }
}
