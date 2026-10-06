using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.BaseConhecimento;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Base de conhecimento: artigos de autoajuda com pesquisa por pontuação.</summary>
[ApiController]
[Route("api/base-conhecimento")]
[Tags("Base de Conhecimento")]
public class BaseConhecimentoController(IBaseConhecimentoService baseConhecimentoService) : ControllerBase
{
    /// <summary>Lista todos os artigos (visível a qualquer perfil autenticado).</summary>
    /// <response code="200">Lista de artigos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ArtigoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ArtigoDto>>> Get(CancellationToken ct)
        => Ok(await baseConhecimentoService.GetAllAsync(ct));

    /// <summary>Publica um artigo (ADMIN/GESTOR/TECNICO).</summary>
    /// <response code="201">Artigo criado.</response>
    /// <response code="400">Título, conteúdo ou categoria em falta.</response>
    /// <response code="403">Perfil sem permissão para publicar.</response>
    [HttpPost]
    [Authorize(Roles = "ADMIN,GESTOR,TECNICO")]
    [ProducesResponseType(typeof(ArtigoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ArtigoDto>> Create(CreateArtigoRequest request, CancellationToken ct)
    {
        var dto = await baseConhecimentoService.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { }, dto);
    }

    /// <summary>Pesquisa artigos pelo query string "q": no máximo 3 resultados, por pontuação decrescente. Com menos de 3 caracteres devolve uma lista vazia.</summary>
    /// <response code="200">Artigos mais relevantes (array simples, nunca 400).</response>
    /// <summary>"Isto ajudou?" — avaliação do artigo pelo utilizador autenticado (uma por pessoa).</summary>
    /// <response code="200">Artigo com as contagens atualizadas.</response>
    /// <response code="404">Artigo inexistente.</response>
    [HttpPost("{id:guid}/avaliacao")]
    [ProducesResponseType(typeof(ArtigoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtigoDto>> Avaliar(Guid id, AvaliarArtigoRequest request, CancellationToken ct)
        => Ok(await baseConhecimentoService.AvaliarAsync(User.GetUserId(), id, request.Util, ct));

    [HttpGet("buscar")]
    [ProducesResponseType(typeof(List<ArtigoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ArtigoDto>>> Buscar([FromQuery] string? q, CancellationToken ct)
        => Ok(await baseConhecimentoService.BuscarAsync(q, ct));
}
