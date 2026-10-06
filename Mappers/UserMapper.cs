using Novati.API.Dtos.Users;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Mappers;

/// <summary>Converte User ⇄ DTO.</summary>
public static class UserMapper
{
    public static UserDto ToDto(User u) => new(
        Id: u.Id,
        Nome: u.Nome,
        Email: u.Email,
        Role: u.Role.ToString(),
        AssinaturaFicheiroId: u.AssinaturaFicheiroId
    );

    /// <summary>Monta a entidade a partir do request. Role e hash já vêm resolvidos pelo Service.</summary>
    public static User ToEntity(CreateUserRequest r, Role role, string passwordHash) => new()
    {
        Nome = r.Nome,
        Email = r.Email,
        Role = role,
        PasswordHash = passwordHash,
    };

    /// <summary>Aplica os campos do request à entidade existente. Role já vem resolvida pelo Service.</summary>
    public static void ApplyUpdate(User u, UpdateUserRequest r, Role role)
    {
        u.Nome = r.Nome;
        u.Email = r.Email;
        u.Role = role;
    }
}