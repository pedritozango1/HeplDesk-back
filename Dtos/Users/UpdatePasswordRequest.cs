using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Users;

/// <summary>Pedido de alteração da própria password.</summary>
public class UpdatePasswordRequest
{
    /// <summary>Password em uso — prova que é o próprio a pedir a alteração.</summary>
    [Required]
    public string PasswordAtual { get; set; } = "";

    /// <summary>Password forte: 8+ caracteres, com maiúscula, minúscula, número e símbolo.</summary>
    /// <example>Novati#2026</example>
    [Required]
    public string NovaPassword { get; set; } = "";
}
