using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Auth;

/// <summary>Pedido de login.</summary>
public class LoginRequest
{
    /// <summary>Email do utilizador.</summary>
    /// <example>rita.admin@empresa.com</example>
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    /// <summary>Palavra-passe em texto.</summary>
    /// <example>novati123</example>
    [Required]
    public string Password { get; set; } = "";
}