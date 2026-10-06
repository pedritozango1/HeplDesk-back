using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Chat;

/// <summary>Pedido de envio de mensagem de chat.</summary>
public class CreateMensagemRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid SolicitacaoId { get; set; }

    /// <example>Já experimentou reiniciar o portátil?</example>
    [Required]
    public string Texto { get; set; } = "";
}
