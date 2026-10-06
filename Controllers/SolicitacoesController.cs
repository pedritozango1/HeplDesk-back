using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Solicitacoes;
using Novati.API.Models.Enums;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Solicitações de suporte abertas pelos utilizadores.</summary>
[ApiController]
[Route("api/solicitacoes")]
[Tags("Solicitações")]
public class SolicitacoesController(ISolicitacaoService solicitacaoService) : ControllerBase
{
    /// <summary>Lista as solicitações. FUNCIONARIO só vê as suas; os restantes perfis veem todas.</summary>
    /// <response code="200">Lista de solicitações (nunca 403 — filtra em vez de bloquear).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SolicitacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SolicitacaoDto>>> Get(CancellationToken ct)
        => Ok(await solicitacaoService.GetVisiveisAsync(User.GetUserId(), User.GetRoleAsEnum(), ct));

    /// <summary>
    /// Lista paginada de solicitações, mais recentes primeiro. Pesquisa em título e descrição;
    /// filtro por estado. FUNCIONARIO só vê as suas.
    /// </summary>
    /// <response code="200">Uma página de solicitações + total.</response>
    /// <response code="400">Paginação fora dos limites ou estado inválido.</response>
    [HttpGet("paginado")]
    [ProducesResponseType(typeof(PaginaResultado<SolicitacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<SolicitacaoDto>>> GetPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? estado, CancellationToken ct)
        => Ok(await solicitacaoService.GetPaginaAsync(User.GetUserId(), User.GetRoleAsEnum(), paginacao, estado, ct));

    /// <summary>Cria uma solicitação em estado ABERTA e notifica todos os técnicos.</summary>
    /// <response code="201">Solicitação criada.</response>
    /// <response code="400">Prioridade inválida ou campos obrigatórios em falta.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SolicitacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SolicitacaoDto>> Create(CreateSolicitacaoRequest request, CancellationToken ct)
    {
        var dto = await solicitacaoService.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { }, dto);
    }

    /// <summary>Fecha uma solicitação. Só o solicitante, um gestor ou um admin, e apenas se estiver RESOLVIDA.</summary>
    /// <response code="200">Solicitação fechada.</response>
    /// <response code="400">A solicitação não está resolvida.</response>
    /// <response code="403">Sem permissão para fechar.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    [HttpPost("{id:guid}/fechar")]
    [ProducesResponseType(typeof(SolicitacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitacaoDto>> Fechar(Guid id, CancellationToken ct)
        => Ok(await solicitacaoService.FecharAsync(id, User.GetUserId(), User.GetRoleAsEnum(), ct));

    /// <summary>Avalia uma solicitação resolvida ou fechada. Só o solicitante, e apenas uma vez.</summary>
    /// <response code="200">Solicitação com a avaliação registada.</response>
    /// <response code="400">Fora do estado permitido, já avaliada, ou estrelas fora de 1–5.</response>
    /// <response code="403">Só o solicitante pode avaliar.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    [HttpPost("{id:guid}/avaliar")]
    [ProducesResponseType(typeof(SolicitacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitacaoDto>> Avaliar(Guid id, AvaliarSolicitacaoRequest request, CancellationToken ct)
        => Ok(await solicitacaoService.AvaliarAsync(id, User.GetUserId(), request, ct));

    /// <summary>O solicitante marca a própria solicitação como resolvida pela base de conhecimento.</summary>
    /// <response code="200">Solicitação resolvida via base.</response>
    /// <response code="400">A solicitação não está ABERTA.</response>
    /// <response code="403">Só o solicitante pode fazer isto.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    [HttpPost("{id:guid}/resolver-base")]
    [ProducesResponseType(typeof(SolicitacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitacaoDto>> ResolverBase(Guid id, CancellationToken ct)
        => Ok(await solicitacaoService.ResolverViaBaseAsync(id, User.GetUserId(), ct));
}
