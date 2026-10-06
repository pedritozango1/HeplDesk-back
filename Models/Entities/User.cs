using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;
/// <summary>Utilizador do sistema.</summary>
public class User : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    // Assinatura digital (imagem) → Ficheiro. Null = ainda não definiu assinatura.
    public Guid? AssinaturaFicheiroId { get; set; }
    public Ficheiro? AssinaturaFicheiro { get; set; }
}
