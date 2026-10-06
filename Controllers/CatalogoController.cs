using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Catalogo;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Catálogo: modelos de dispositivo, modelos de componente e compatibilidades.</summary>
[ApiController]
[Route("api/catalogo")]
[Tags("Catálogo")]
public class CatalogoController(ICatalogoService catalogoService) : ControllerBase
{
    // ─── Modelos de dispositivo ─────────────────────────

    /// <summary>Lista todos os modelos de dispositivo.</summary>
    /// <response code="200">Lista de modelos de dispositivo.</response>
    [HttpGet("modelos-dispositivo")]
    [ProducesResponseType(typeof(List<ModeloDispositivoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ModeloDispositivoDto>>> GetModelosDispositivo(CancellationToken ct)
        => Ok(await catalogoService.GetModelosDispositivoAsync(ct));

    /// <summary>Lista paginada de modelos de dispositivo. Pesquisa em nome e fabricante; filtro por tipo.</summary>
    /// <response code="200">Uma página de modelos + total.</response>
    /// <response code="400">Página ou tamanho de página fora dos limites.</response>
    [HttpGet("modelos-dispositivo/paginado")]
    [ProducesResponseType(typeof(PaginaResultado<ModeloDispositivoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<ModeloDispositivoDto>>> GetModelosDispositivoPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? tipo, CancellationToken ct)
        => Ok(await catalogoService.GetModelosDispositivoPaginaAsync(paginacao, tipo, ct));

    /// <summary>Cria um modelo de dispositivo (apenas ADMIN).</summary>
    /// <response code="201">Modelo criado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    [HttpPost("modelos-dispositivo")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ModeloDispositivoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ModeloDispositivoDto>> CreateModeloDispositivo(CreateModeloDispositivoRequest request, CancellationToken ct)
    {
        var dto = await catalogoService.CreateModeloDispositivoAsync(request, ct);
        return CreatedAtAction(nameof(GetModelosDispositivo), new { }, dto);
    }

    /// <summary>Atualiza um modelo de dispositivo (apenas ADMIN).</summary>
    /// <response code="200">Modelo atualizado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo não encontrado.</response>
    [HttpPatch("modelos-dispositivo/{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ModeloDispositivoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModeloDispositivoDto>> UpdateModeloDispositivo(Guid id, UpdateModeloDispositivoRequest request, CancellationToken ct)
        => Ok(await catalogoService.UpdateModeloDispositivoAsync(id, request, ct));

    /// <summary>Remove um modelo de dispositivo (apenas ADMIN).</summary>
    /// <response code="204">Modelo removido.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo não encontrado.</response>
    /// <response code="409">Existem dispositivos deste modelo.</response>
    [HttpDelete("modelos-dispositivo/{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteModeloDispositivo(Guid id, CancellationToken ct)
    {
        await catalogoService.DeleteModeloDispositivoAsync(id, ct);
        return NoContent();
    }

    // ─── Modelos de componente ──────────────────────────

    /// <summary>Lista todos os modelos de componente.</summary>
    /// <response code="200">Lista de modelos de componente.</response>
    [HttpGet("modelos-componente")]
    [ProducesResponseType(typeof(List<ModeloComponenteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ModeloComponenteDto>>> GetModelosComponente(CancellationToken ct)
        => Ok(await catalogoService.GetModelosComponenteAsync(ct));

    /// <summary>Lista paginada de modelos de componente. Pesquisa em nome e capacidade; filtro por tipo.</summary>
    /// <response code="200">Uma página de modelos + total.</response>
    /// <response code="400">Página ou tamanho de página fora dos limites.</response>
    [HttpGet("modelos-componente/paginado")]
    [ProducesResponseType(typeof(PaginaResultado<ModeloComponenteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<ModeloComponenteDto>>> GetModelosComponentePaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? tipo, CancellationToken ct)
        => Ok(await catalogoService.GetModelosComponentePaginaAsync(paginacao, tipo, ct));

    /// <summary>Cria um modelo de componente (apenas ADMIN). <c>stockMinimo</c> assume 1 se omitido.</summary>
    /// <response code="201">Modelo criado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    [HttpPost("modelos-componente")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ModeloComponenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ModeloComponenteDto>> CreateModeloComponente(CreateModeloComponenteRequest request, CancellationToken ct)
    {
        var dto = await catalogoService.CreateModeloComponenteAsync(request, ct);
        return CreatedAtAction(nameof(GetModelosComponente), new { }, dto);
    }

    /// <summary>Atualiza um modelo de componente (apenas ADMIN).</summary>
    /// <response code="200">Modelo atualizado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo não encontrado.</response>
    [HttpPatch("modelos-componente/{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ModeloComponenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModeloComponenteDto>> UpdateModeloComponente(Guid id, UpdateModeloComponenteRequest request, CancellationToken ct)
        => Ok(await catalogoService.UpdateModeloComponenteAsync(id, request, ct));

    /// <summary>Remove um modelo de componente (apenas ADMIN).</summary>
    /// <response code="204">Modelo removido.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo não encontrado.</response>
    /// <response code="409">Existem instâncias ou stock associados a este modelo.</response>
    [HttpDelete("modelos-componente/{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteModeloComponente(Guid id, CancellationToken ct)
    {
        await catalogoService.DeleteModeloComponenteAsync(id, ct);
        return NoContent();
    }

    // ─── Compatibilidades ───────────────────────────────

    /// <summary>Lista todas as compatibilidades entre modelos.</summary>
    /// <response code="200">Lista de compatibilidades.</response>
    [HttpGet("compatibilidades")]
    [ProducesResponseType(typeof(List<CompatibilidadeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CompatibilidadeDto>>> GetCompatibilidades(CancellationToken ct)
        => Ok(await catalogoService.GetCompatibilidadesAsync(ct));

    /// <summary>Cria uma compatibilidade entre um modelo de dispositivo e um modelo de componente (apenas ADMIN).</summary>
    /// <response code="201">Compatibilidade criada.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo de dispositivo ou componente não encontrado.</response>
    /// <response code="409">Esta compatibilidade já existe.</response>
    [HttpPost("compatibilidades")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(CompatibilidadeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompatibilidadeDto>> CreateCompatibilidade(CreateCompatibilidadeRequest request, CancellationToken ct)
    {
        var dto = await catalogoService.CreateCompatibilidadeAsync(request, ct);
        return CreatedAtAction(nameof(GetCompatibilidades), new { }, dto);
    }

    /// <summary>Remove uma compatibilidade (apenas ADMIN).</summary>
    /// <response code="204">Compatibilidade removida.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Compatibilidade não encontrada.</response>
    [HttpDelete("compatibilidades/{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCompatibilidade(Guid id, CancellationToken ct)
    {
        await catalogoService.DeleteCompatibilidadeAsync(id, ct);
        return NoContent();
    }
}
