namespace Novati.API.Dtos.Users;

/// <summary>Dados de um utilizador expostos pela API (nunca inclui PasswordHash).</summary>
public record UserDto(
    Guid Id,
    string Nome,
    string Email,
    string Role,
    /// <summary>Imagem da assinatura (GET /api/ficheiros/{id}/conteudo) ou null.</summary>
    Guid? AssinaturaFicheiroId
);