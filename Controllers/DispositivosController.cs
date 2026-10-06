using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Dispositivos;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Inventário: localizações, dispositivos físicos e instâncias de componente.</summary>
[ApiController]
[Route("api/dispositivos")]
[Tags("Dispositivos")]
public class DispositivosController(IDispositivoService dispositivoService) : ControllerBase
{
    // ─── Localizações ───────────────────────────────────

    /// <summary>Lista todas as localizações.</summary>
    /// <response code="200">Lista de localizações.</response>
    [HttpGet("localizacoes")]
    [ProducesResponseType(typeof(List<LocalizacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LocalizacaoDto>>> GetLocalizacoes(CancellationToken ct)
        => Ok(await dispositivoService.GetLocalizacoesAsync(ct));

    /// <summary>Cria uma localização (ADMIN/TECNICO).</summary>
    /// <response code="201">Localização criada.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Localização-pai não encontrada.</response>
    [HttpPost("localizacoes")]
    [Authorize(Roles = "ADMIN,TECNICO")]
    [ProducesResponseType(typeof(LocalizacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalizacaoDto>> CreateLocalizacao(CreateLocalizacaoRequest request, CancellationToken ct)
    {
        var dto = await dispositivoService.CreateLocalizacaoAsync(request, ct);
        return CreatedAtAction(nameof(GetLocalizacoes), new { }, dto);
    }

    // ─── Dispositivos físicos ───────────────────────────

    /// <summary>Lista todos os dispositivos físicos.</summary>
    /// <response code="200">Lista de dispositivos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DispositivoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DispositivoDto>>> GetDispositivos(CancellationToken ct)
        => Ok(await dispositivoService.GetDispositivosAsync(ct));

    /// <summary>
    /// Lista paginada de dispositivos. Pesquisa em património, modelo, localização e responsável;
    /// filtro por estado.
    /// </summary>
    /// <response code="200">Uma página de dispositivos + total.</response>
    /// <response code="400">Paginação fora dos limites ou estado inválido.</response>
    [HttpGet("paginado")]
    [ProducesResponseType(typeof(PaginaResultado<DispositivoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<DispositivoDto>>> GetDispositivosPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? estado, CancellationToken ct)
        => Ok(await dispositivoService.GetDispositivosPaginaAsync(paginacao, estado, ct));

    /// <summary>Cria um dispositivo físico (ADMIN/TECNICO).</summary>
    /// <response code="201">Dispositivo criado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo de dispositivo ou localização não encontrados.</response>
    /// <response code="409">Já existe um dispositivo com este nº de património.</response>
    [HttpPost]
    [Authorize(Roles = "ADMIN,TECNICO")]
    [ProducesResponseType(typeof(DispositivoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DispositivoDto>> CreateDispositivo(CreateDispositivoRequest request, CancellationToken ct)
    {
        var dto = await dispositivoService.CreateDispositivoAsync(request, ct);
        return CreatedAtAction(nameof(GetDispositivos), new { }, dto);
    }

    /// <summary>Muda o estado de um dispositivo (ADMIN/TECNICO).</summary>
    /// <response code="200">Dispositivo atualizado.</response>
    /// <response code="400">Estado inválido.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Dispositivo não encontrado.</response>
    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = "ADMIN,TECNICO")]
    [ProducesResponseType(typeof(DispositivoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DispositivoDto>> UpdateEstado(Guid id, UpdateEstadoDispositivoRequest request, CancellationToken ct)
        => Ok(await dispositivoService.UpdateEstadoAsync(id, request, ct));

    // ─── Instâncias de componente ───────────────────────

    /// <summary>Lista todas as instâncias de componente instaladas.</summary>
    /// <response code="200">Lista de instâncias.</response>
    [HttpGet("instancias")]
    [ProducesResponseType(typeof(List<InstanciaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InstanciaDto>>> GetInstancias(CancellationToken ct)
        => Ok(await dispositivoService.GetInstanciasAsync(ct));

    /// <summary>Instala um componente num dispositivo (ADMIN/TECNICO). Valida compatibilidade com o modelo.</summary>
    /// <response code="201">Instância criada.</response>
    /// <response code="400">Componente incompatível com este modelo.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Dispositivo ou modelo de componente não encontrado.</response>
    [HttpPost("{id:guid}/instancias")]
    [Authorize(Roles = "ADMIN,TECNICO")]
    [ProducesResponseType(typeof(InstanciaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstanciaDto>> CreateInstancia(Guid id, CreateInstanciaRequest request, CancellationToken ct)
    {
        var dto = await dispositivoService.CreateInstanciaAsync(id, request, ct);
        return CreatedAtAction(nameof(GetInstancias), new { }, dto);
    }
}
