using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.Chat;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Chat associado às solicitações. Cada conversa é uma solicitação.</summary>
[ApiController]
[Route("api/chat")]
[Tags("Chat")]
public class ChatController(IChatService chatService) : ControllerBase
{
    /// <summary>Resumo das conversas visíveis ao utilizador: mensagens por ler e última mensagem de cada uma.</summary>
    /// <response code="200">Resumos (só conversas que já têm mensagens; nunca 403).</response>
    [HttpGet("conversas")]
    [ProducesResponseType(typeof(List<ConversaResumoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConversaResumoDto>>> GetConversas(CancellationToken ct)
        => Ok(await chatService.GetConversasAsync(User.GetUserId(), User.GetRoleAsEnum(), ct));

    /// <summary>Histórico de uma conversa aos blocos, a contar da mensagem mais recente.</summary>
    /// <response code="200">Bloco por ordem cronológica + se ainda há mensagens mais antigas.</response>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    [HttpGet("conversas/{solicitacaoId:guid}/mensagens")]
    [ProducesResponseType(typeof(MensagensPaginaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MensagensPaginaDto>> GetMensagensDaConversa(Guid solicitacaoId, [FromQuery] MensagensQuery query, CancellationToken ct)
        => Ok(await chatService.GetMensagensDaConversaAsync(User.GetUserId(), User.GetRoleAsEnum(), solicitacaoId, query, ct));

    /// <summary>Envia uma mensagem numa conversa e notifica a outra parte.</summary>
    /// <response code="201">Mensagem enviada.</response>
    /// <response code="400">Texto vazio.</response>
    /// <response code="403">Sem acesso a esta conversa.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    [HttpPost("mensagens")]
    [ProducesResponseType(typeof(MensagemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MensagemDto>> CriarMensagem(CreateMensagemRequest request, CancellationToken ct)
    {
        var dto = await chatService.CriarAsync(User.GetUserId(), User.GetRoleAsEnum(), request, ct);
        return CreatedAtAction(nameof(GetMensagensDaConversa), new { solicitacaoId = dto.SolicitacaoId }, dto);
    }

    /// <summary>Marca como lidas as mensagens de outros nesta conversa.</summary>
    /// <response code="204">Conversa marcada como lida.</response>
    [HttpPatch("conversas/{solicitacaoId:guid}/lidas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarcarConversaLida(Guid solicitacaoId, CancellationToken ct)
    {
        await chatService.MarcarConversaLidaAsync(User.GetUserId(), solicitacaoId, ct);
        return NoContent();
    }
}
