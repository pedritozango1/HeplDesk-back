using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Compras;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Compras: requisições de peças criadas quando falta stock.</summary>
[ApiController]
[Route("api/compras")]
[Tags("Compras")]
public class ComprasController(IComprasService comprasService) : ControllerBase
{
    /// <summary>Lista as requisições de compra. Fora de ADMIN/GESTOR devolve lista vazia (nunca 403).</summary>
    /// <response code="200">Lista de requisições, em ordem de criação crescente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RequisicaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RequisicaoDto>>> Get(CancellationToken ct)
        => Ok(await comprasService.GetVisiveisAsync(User.GetRoleAsEnum(), ct));

    /// <summary>
    /// Lista paginada de requisições, mais recentes primeiro. Pesquisa em componente, justificativa
    /// e solicitante; filtro por estado. Fora de ADMIN/GESTOR devolve página vazia (nunca 403).
    /// </summary>
    /// <response code="200">Uma página de requisições + total.</response>
    /// <response code="400">Paginação fora dos limites ou estado inválido.</response>
    [HttpGet("paginado")]
    [ProducesResponseType(typeof(PaginaResultado<RequisicaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<RequisicaoDto>>> GetPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? estado, CancellationToken ct)
        => Ok(await comprasService.GetPaginaAsync(User.GetRoleAsEnum(), paginacao, estado, ct));

    /// <summary>Aprova uma requisição PENDENTE e notifica quem a pediu.</summary>
    /// <response code="200">Requisição aprovada.</response>
    /// <response code="400">A requisição não está pendente.</response>
    /// <response code="403">Perfil sem permissão.</response>
    /// <response code="404">Requisição não encontrada.</response>
    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "ADMIN,GESTOR")]
    [ProducesResponseType(typeof(RequisicaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequisicaoDto>> Aprovar(Guid id, CancellationToken ct)
        => Ok(await comprasService.AprovarAsync(id, ct));

    /// <summary>Dá entrada das peças aprovadas em stock (cria as unidades DISPONIVEL + movimento ENTRADA).</summary>
    /// <response code="200">Requisição marcada como ENTREGUE.</response>
    /// <response code="400">A requisição não está aprovada.</response>
    /// <response code="403">Perfil sem permissão.</response>
    /// <response code="404">Requisição não encontrada.</response>
    [HttpPost("{id:guid}/entrada")]
    [Authorize(Roles = "ADMIN,GESTOR")]
    [ProducesResponseType(typeof(RequisicaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequisicaoDto>> Entrada(Guid id, CancellationToken ct)
        => Ok(await comprasService.DarEntradaAsync(id, ct));

    /// <summary>Recusa uma requisição PENDENTE.</summary>
    /// <response code="200">Requisição recusada.</response>
    /// <response code="400">A requisição não está pendente.</response>
    /// <response code="403">Perfil sem permissão.</response>
    /// <response code="404">Requisição não encontrada.</response>
    [HttpPost("{id:guid}/recusar")]
    [Authorize(Roles = "ADMIN,GESTOR")]
    [ProducesResponseType(typeof(RequisicaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequisicaoDto>> Recusar(Guid id, CancellationToken ct)
        => Ok(await comprasService.RecusarAsync(id, ct));
}
