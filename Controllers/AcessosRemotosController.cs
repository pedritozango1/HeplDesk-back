using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.AcessoRemoto;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Acesso remoto ao PC do solicitante: pedido, consentimento, token de uso único e sessão.</summary>
[ApiController]
[Route("api/acessos-remotos")]
[Tags("Acesso remoto")]
public class AcessosRemotosController(IAcessoRemotoService acessoRemotoService) : ControllerBase
{
    /// <summary>Lista os pedidos e sessões. FUNCIONARIO vê os das suas solicitações; TECNICO os que pediu; GESTOR e ADMIN todos.</summary>
    /// <response code="200">Lista de sessões (nunca 403 — filtra em vez de bloquear).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SessaoRemotaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SessaoRemotaDto>>> Get(CancellationToken ct)
        => Ok(await acessoRemotoService.GetVisiveisAsync(User.GetUserId(), User.GetRoleAsEnum(), ct));

    /// <summary>O técnico que assumiu a solicitação pede acesso ao PC do solicitante. Fica PEDIDA e notifica-o.</summary>
    /// <response code="201">Pedido criado.</response>
    /// <response code="400">Solicitação fora de atendimento ou modo inválido.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    /// <response code="409">Já existe um pedido em curso para esta solicitação.</response>
    [HttpPost("/api/solicitacoes/{solicitacaoId:guid}/acesso-remoto")]
    [ProducesResponseType(typeof(SessaoRemotaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessaoRemotaDto>> Pedir(Guid solicitacaoId, [FromBody] PedirAcessoRequest? request, CancellationToken ct)
    {
        var dto = await acessoRemotoService.PedirAsync(User.GetUserId(), solicitacaoId, request, ct);
        return CreatedAtAction(nameof(Get), new { }, dto);
    }

    /// <summary>
    /// O solicitante autoriza o pedido. Gera o token de uso único, devolve-o (única vez)
    /// e entrega-o ao técnico em tempo real.
    /// </summary>
    /// <remarks>No modo CONTROLAR o corpo leva o código que o Agente Novati mostra no PC do solicitante.</remarks>
    /// <response code="200">Sessão AUTORIZADA + token + validade.</response>
    /// <response code="400">Modo CONTROLAR sem código de agente, ou sem nenhum agente ligado com esse código.</response>
    /// <response code="403">Só o solicitante pode autorizar.</response>
    /// <response code="404">Sessão não encontrada.</response>
    /// <response code="409">O pedido já foi respondido.</response>
    /// <response code="410">O pedido expirou.</response>
    [HttpPost("{id:guid}/autorizar")]
    [ProducesResponseType(typeof(AutorizacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<AutorizacaoResponse>> Autorizar(Guid id, [FromBody] AutorizarRequest? request, CancellationToken ct)
        => Ok(await acessoRemotoService.AutorizarAsync(
            User.GetUserId(), id, request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct));

    /// <summary>O solicitante recusa o pedido. O motivo é opcional.</summary>
    /// <response code="200">Sessão RECUSADA.</response>
    /// <response code="403">Só o solicitante pode recusar.</response>
    /// <response code="404">Sessão não encontrada.</response>
    /// <response code="409">O pedido já foi respondido.</response>
    /// <response code="410">O pedido expirou.</response>
    [HttpPost("{id:guid}/recusar")]
    [ProducesResponseType(typeof(SessaoRemotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<SessaoRemotaDto>> Recusar(Guid id, [FromBody] MotivoRequest? request, CancellationToken ct)
        => Ok(await acessoRemotoService.RecusarAsync(User.GetUserId(), id, request, ct));

    /// <summary>
    /// O técnico troca o token pela sessão ATIVA. Funciona uma única vez; 5 códigos errados
    /// invalidam o pedido.
    /// </summary>
    /// <response code="200">Sessão ATIVA + servidores ICE (modo VER; vazio no modo CONTROLAR, em que o agente começa a transmitir).</response>
    /// <response code="400">Código com formato errado ou incorreto (a mensagem diz quantas tentativas restam).</response>
    /// <response code="403">Não é o técnico que pediu o acesso.</response>
    /// <response code="404">Sessão não encontrada.</response>
    /// <response code="409">Código já utilizado, ou sessão ainda não autorizada / já terminada.</response>
    /// <response code="410">O código expirou.</response>
    /// <response code="429">Tentativas esgotadas, ou demasiados pedidos por minuto.</response>
    [HttpPost("{id:guid}/resgatar")]
    [EnableRateLimiting("resgate")]
    [ProducesResponseType(typeof(ResgateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ResgateResponse>> Resgatar(Guid id, ResgatarRequest request, CancellationToken ct)
        => Ok(await acessoRemotoService.ResgatarAsync(User.GetUserId(), id, request, ct));

    /// <summary>
    /// Termina uma sessão ATIVA (fica TERMINADA) ou desiste de um pedido que ainda não começou
    /// (fica CANCELADA). Qualquer participante, um gestor ou um admin.
    /// </summary>
    /// <response code="200">Sessão fechada.</response>
    /// <response code="403">Sem permissão.</response>
    /// <response code="404">Sessão não encontrada.</response>
    /// <response code="409">A sessão já tinha terminado.</response>
    [HttpPost("{id:guid}/terminar")]
    [ProducesResponseType(typeof(SessaoRemotaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessaoRemotaDto>> Terminar(Guid id, [FromBody] MotivoRequest? request, CancellationToken ct)
        => Ok(await acessoRemotoService.TerminarAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Servidores ICE (STUN/TURN) de uma sessão ATIVA. O solicitante usa esta rota; o técnico já os recebe no resgate.</summary>
    /// <response code="200">Servidores ICE + momento em que a sessão termina sozinha.</response>
    /// <response code="403">Não participa na sessão.</response>
    /// <response code="404">Sessão não encontrada.</response>
    /// <response code="409">A sessão não está ativa.</response>
    [HttpGet("{id:guid}/ice")]
    [ProducesResponseType(typeof(IceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IceResponse>> GetIce(Guid id, CancellationToken ct)
        => Ok(await acessoRemotoService.GetIceAsync(User.GetUserId(), id, ct));
}
