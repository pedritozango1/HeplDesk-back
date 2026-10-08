using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Users;

/// <summary>Dados pessoais que o próprio utilizador pode alterar no perfil (o perfil de acesso só o ADMIN muda).</summary>
public class UpdateMeRequest
{
    /// <summary>Nome completo.</summary>
    /// <example>João Técnico</example>
    [Required]
    public string Nome { get; set; } = "";

    /// <summary>Email — é com ele que se inicia sessão; tem de ser único no sistema.</summary>
    /// <example>joao.tecnico@empresa.com</example>
    [Required, EmailAddress]
    public string Email { get; set; } = "";
}
