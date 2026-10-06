namespace Novati.API.Dtos.Users;
/// <summary>Pedido de atualização da assinatura do próprio utilizador.</summary>
public class SignatureRequest
{
    /// <summary>Id de uma imagem já carregada em POST /api/ficheiros, ou null para remover a assinatura.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? FicheiroId { get; set; }
}
