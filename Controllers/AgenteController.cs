using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.Agente;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Distribuição do Agente Novati (o programa de controlo remoto que corre no PC do funcionário).</summary>
[ApiController]
[Route("api/agente")]
[Tags("Agente")]
public class AgenteController(IAgenteService agenteService) : ControllerBase
{
    /// <summary>
    /// Gera um link de download com validade. Com <c>funcionarioId</c>, o link é enviado a esse
    /// utilizador por notificação; sem ele, fica só para copiar.
    /// </summary>
    /// <response code="201">Link gerado.</response>
    /// <response code="403">Só TECNICO, GESTOR e ADMIN.</response>
    /// <response code="404">Agente não publicado nesta instalação, ou utilizador inexistente.</response>
    [HttpPost("links")]
    [Authorize(Roles = "TECNICO,GESTOR,ADMIN")]
    [ProducesResponseType(typeof(LinkAgenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkAgenteDto>> CriarLink([FromBody] CriarLinkAgenteRequest? request, CancellationToken ct)
    {
        var dto = await agenteService.CriarLinkAsync(User.GetUserId(), request?.FuncionarioId, ct);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    /// <summary>Diz se o agente está disponível e se o link ainda é válido (para a página de download).</summary>
    /// <response code="200">Estado do link.</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(AgenteInfoDto), StatusCodes.Status200OK)]
    public ActionResult<AgenteInfoDto> Info([FromQuery] string? t)
        => Ok(agenteService.Info(t));

    /// <summary>
    /// Descarrega o agente. Sem login: quem autoriza é o link (assinado e com validade), para o
    /// browser poder descarregar diretamente um ficheiro grande.
    /// </summary>
    /// <response code="200">O executável, já configurado para este servidor.</response>
    /// <response code="403">Link inválido.</response>
    /// <response code="404">Agente não publicado nesta instalação.</response>
    /// <response code="410">Link expirado.</response>
    [HttpGet("download")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> Download([FromQuery] string? t, CancellationToken ct)
    {
        // Atrás de um proxy que termina o HTTPS, o pedido chega em http: o protocolo real vem neste cabeçalho.
        var protocolo = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
        var (caminho, marca) = agenteService.ParaDownload(t, $"{protocolo}://{Request.Host}");

        Response.ContentType = "application/octet-stream";
        Response.ContentLength = new FileInfo(caminho).Length + marca.Length;
        Response.Headers.ContentDisposition = "attachment; filename=\"NovatiAgente.exe\"";

        // O executável tal como foi publicado, seguido da marca com o endereço do servidor.
        await Response.SendFileAsync(caminho, ct);
        await Response.Body.WriteAsync(marca, ct);
        return new EmptyResult();
    }
}
