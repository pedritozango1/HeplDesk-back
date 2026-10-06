using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.Notificacoes;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Notificações do utilizador autenticado.</summary>
[ApiController]
[Route("api/notificacoes")]
[Tags("Notificações")]
public class NotificacoesController(INotificacaoService notificacaoService) : ControllerBase
{
    /// <summary>Lista as notificações do utilizador do token, em ordem cronológica crescente.</summary>
    /// <response code="200">Lista de notificações.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificacaoDto>>> Get(CancellationToken ct)
        => Ok(await notificacaoService.GetMinhasAsync(User.GetUserId(), ct));

    /// <summary>Marca uma notificação como lida (só pode marcar as suas).</summary>
    /// <response code="204">Notificação marcada.</response>
    /// <response code="403">A notificação não pertence ao utilizador.</response>
    /// <response code="404">Notificação não encontrada.</response>
    [HttpPatch("{id:guid}/lida")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarLida(Guid id, CancellationToken ct)
    {
        await notificacaoService.MarcarLidaAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

    /// <summary>Marca todas as notificações do utilizador como lidas.</summary>
    /// <response code="204">Notificações marcadas.</response>
    [HttpPatch("lidas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarcarTodasLidas(CancellationToken ct)
    {
        await notificacaoService.MarcarTodasLidasAsync(User.GetUserId(), ct);
        return NoContent();
    }
}
