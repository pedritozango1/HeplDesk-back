using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Users;

/// <summary>Pedido de criação de utilizador (só ADMIN).</summary>
public class CreateUserRequest
{
    /// <summary>Nome completo.</summary>
    /// <example>João Técnico</example>
    [Required]
    public string Nome { get; set; } = "";

    /// <summary>Email — tem de ser único no sistema.</summary>
    /// <example>joao.tecnico@empresa.com</example>
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    /// <summary>Role: ADMIN, GESTOR, TECNICO ou FUNCIONARIO.</summary>
    /// <example>TECNICO</example>
    [Required]
    public string Role { get; set; } = "";
}
