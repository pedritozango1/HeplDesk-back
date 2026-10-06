using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.RelatoriosTecnicos;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Relatórios técnicos: documento formal que fecha uma ordem resolvida.</summary>
[ApiController]
[Route("api/relatorios-tecnicos")]
[Tags("Relatórios Técnicos")]
public class RelatoriosTecnicosController(IRelatorioService relatorioService) : ControllerBase
{
    /// <summary>Lista os relatórios. ADMIN/TECNICO veem todos, GESTOR só finalizados/aprovados, FUNCIONARIO só aprovados das suas.</summary>
    /// <response code="200">Lista de relatórios (nunca 403 — filtra por perfil).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RelatorioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RelatorioDto>>> Get(CancellationToken ct)
        => Ok(await relatorioService.GetVisiveisAsync(User.GetUserId(), User.GetRoleAsEnum(), ct));

    /// <summary>Consulta um relatório. 404 se não existir ou o perfil não lhe ter acesso.</summary>
    /// <response code="200">Relatório técnico.</response>
    /// <response code="404">Relatório não encontrado ou sem acesso.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RelatorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelatorioDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await relatorioService.GetByIdAsync(User.GetUserId(), User.GetRoleAsEnum(), id, ct));

    /// <summary>Cria um relatório (rascunho) a partir de uma ordem RESOLVIDO.</summary>
    /// <response code="201">Relatório criado.</response>
    /// <response code="400">A ordem não está resolvida ou falta o id da ordem.</response>
    /// <response code="403">Não é o técnico da reparação.</response>
    /// <response code="404">Ordem não encontrada.</response>
    /// <response code="409">A ordem já tem relatório técnico.</response>
    [HttpPost]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(RelatorioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RelatorioDto>> Create(CreateRelatorioRequest request, CancellationToken ct)
    {
        var dto = await relatorioService.CreateAsync(User.GetUserId(), User.GetRoleAsEnum(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Atualiza o relatório e regista uma linha de histórico por cada campo alterado.</summary>
    /// <response code="200">Relatório atualizado.</response>
    /// <response code="400">O relatório está aprovado e já não é editável.</response>
    /// <response code="403">Não é o autor do relatório.</response>
    /// <response code="404">Relatório não encontrado.</response>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "TECNICO,ADMIN,GESTOR")]
    [ProducesResponseType(typeof(RelatorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelatorioDto>> Update(Guid id, UpdateRelatorioRequest request, CancellationToken ct)
        => Ok(await relatorioService.UpdateAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Finaliza o relatório (rascunho → finalizado) e notifica os gestores.</summary>
    /// <response code="200">Relatório finalizado.</response>
    /// <response code="400">O relatório não está em rascunho.</response>
    /// <response code="403">Não é o autor do relatório.</response>
    /// <response code="404">Relatório não encontrado.</response>
    [HttpPost("{id:guid}/finalizar")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(RelatorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelatorioDto>> Finalizar(Guid id, CancellationToken ct)
        => Ok(await relatorioService.FinalizarAsync(User.GetUserId(), User.GetRoleAsEnum(), id, ct));

    /// <summary>Reabre o relatório para edição (finalizado → rascunho).</summary>
    /// <response code="200">Relatório reaberto.</response>
    /// <response code="400">O relatório não está finalizado.</response>
    /// <response code="403">Não é o autor do relatório.</response>
    /// <response code="404">Relatório não encontrado.</response>
    [HttpPost("{id:guid}/reabrir")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(RelatorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelatorioDto>> Reabrir(Guid id, CancellationToken ct)
        => Ok(await relatorioService.ReabrirAsync(User.GetUserId(), User.GetRoleAsEnum(), id, ct));

    /// <summary>Aprova o relatório (finalizado → aprovado) e notifica o autor.</summary>
    /// <response code="200">Relatório aprovado.</response>
    /// <response code="400">O relatório não está finalizado.</response>
    /// <response code="403">Perfil sem permissão.</response>
    /// <response code="404">Relatório não encontrado.</response>
    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "GESTOR,ADMIN")]
    [ProducesResponseType(typeof(RelatorioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelatorioDto>> Aprovar(Guid id, CancellationToken ct)
        => Ok(await relatorioService.AprovarAsync(User.GetUserId(), User.GetRoleAsEnum(), id, ct));
}
