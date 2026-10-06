using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.Atendimentos;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Atendimentos: ciclo de vida da ordem de reparo.</summary>
[ApiController]
[Route("api/atendimentos")]
[Tags("Atendimentos")]
public class AtendimentosController(IAtendimentoService atendimentoService) : ControllerBase
{
    /// <summary>Lista as ordens. FUNCIONARIO só vê as ordens das suas solicitações (nunca 403).</summary>
    /// <response code="200">Lista de ordens com peças, rejeições e histórico.</response>
    [HttpGet("ordens")]
    [ProducesResponseType(typeof(List<OrdemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrdemDto>>> GetOrdens(CancellationToken ct)
        => Ok(await atendimentoService.GetVisiveisAsync(User.GetUserId(), User.GetRoleAsEnum(), ct));

    /// <summary>Assume uma solicitação ABERTA: cria a ordem em EM_DIAGNOSTICO e notifica o solicitante.</summary>
    /// <response code="201">Ordem criada.</response>
    /// <response code="403">Perfil sem permissão.</response>
    /// <response code="404">Solicitação não encontrada.</response>
    /// <response code="409">A solicitação já foi assumida.</response>
    [HttpPost("solicitacoes/{solicitacaoId:guid}/assumir")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrdemDto>> Assumir(Guid solicitacaoId, CancellationToken ct)
    {
        var dto = await atendimentoService.AssumirAsync(User.GetUserId(), solicitacaoId, ct);
        return CreatedAtAction(nameof(GetOrdens), new { }, dto);
    }

    /// <summary>Regista o texto do diagnóstico. Só em EM_DIAGNOSTICO.</summary>
    /// <response code="200">Ordem atualizada.</response>
    /// <response code="400">Fora do estado permitido ou diagnóstico em falta.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPatch("ordens/{id:guid}/diagnostico")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> Diagnostico(Guid id, DiagnosticoRequest request, CancellationToken ct)
        => Ok(await atendimentoService.GuardarDiagnosticoAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Conclui o diagnóstico e avança a ordem para EM_REPARACAO.</summary>
    /// <response code="200">Ordem avançada.</response>
    /// <response code="400">Fora do estado permitido ou diagnóstico vazio.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPost("ordens/{id:guid}/concluir-diagnostico")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> ConcluirDiagnostico(Guid id, CancellationToken ct)
        => Ok(await atendimentoService.ConcluirDiagnosticoAsync(User.GetUserId(), User.GetRoleAsEnum(), id, ct));

    /// <summary>Acrescenta um comentário ao histórico da ordem.</summary>
    /// <response code="200">Ordem atualizada.</response>
    /// <response code="400">Texto em falta.</response>
    /// <response code="403">Sem permissão para comentar.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPost("ordens/{id:guid}/comentarios")]
    [Authorize(Roles = "TECNICO,ADMIN,GESTOR")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> Comentar(Guid id, ComentarioRequest request, CancellationToken ct)
        => Ok(await atendimentoService.AdicionarComentarioAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Reatribui a ordem a outro técnico (ADMIN/GESTOR).</summary>
    /// <response code="200">Ordem reatribuída.</response>
    /// <response code="400">O novo responsável não é um técnico.</response>
    /// <response code="403">Perfil sem permissão.</response>
    /// <response code="404">Ordem ou técnico não encontrado.</response>
    [HttpPost("ordens/{id:guid}/reatribuir")]
    [Authorize(Roles = "ADMIN,GESTOR")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> Reatribuir(Guid id, ReatribuirRequest request, CancellationToken ct)
        => Ok(await atendimentoService.ReatribuirAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Reserva uma peça para a ordem (só em EM_REPARACAO). A resposta tem duas formas possíveis.</summary>
    /// <remarks>
    /// Com stock: <c>{ "ok": true, "ordem": OrdemDto, "unidadeStockId": "guid", "codigo": "RAM-050" }</c>.<br/>
    /// Sem stock: <c>{ "ok": false, "ordem": OrdemDto, "requisicao": RequisicaoDto, "requisicaoId": "guid" }</c>.<br/>
    /// O Swagger não modela <c>oneOf</c>; ambos os esquemas estão documentados em
    /// <see cref="ReservaOkResponse"/> e <see cref="ReservaFalhaResponse"/>.
    /// </remarks>
    /// <response code="200">Peça reservada (ok = true) ou requisição de compra criada (ok = false).</response>
    /// <response code="400">Ordem fora de EM_REPARACAO.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPost("ordens/{id:guid}/reservar-peca")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(ReservaOkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReservaFalhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReservarPeca(Guid id, ReservarPecaRequest request, CancellationToken ct)
    {
        var resultado = await atendimentoService.ReservarPecaAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct);

        if (resultado.Ok)
            return Ok(new ReservaOkResponse(true, resultado.Ordem, resultado.UnidadeStockId, resultado.Codigo));

        return Ok(new ReservaFalhaResponse(false, resultado.Ordem, resultado.Requisicao!, resultado.Requisicao!.Id));
    }

    /// <summary>Instala a unidade reservada no dispositivo e regista a peça como usada.</summary>
    /// <response code="200">Ordem atualizada.</response>
    /// <response code="400">Unidade não reservada para esta ordem, peça não reservada, componente incompatível ou instância anterior de outro dispositivo.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Ordem, unidade ou dispositivo não encontrado.</response>
    [HttpPost("ordens/{id:guid}/instalar-peca")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> InstalarPeca(Guid id, InstalarPecaRequest request, CancellationToken ct)
        => Ok(await atendimentoService.InstalarPecaAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Propõe a solução: ordem e solicitação passam a AGUARDA_VALIDACAO e o solicitante é notificado.</summary>
    /// <response code="200">Ordem atualizada.</response>
    /// <response code="400">Fora do estado permitido ou tempo gasto inválido.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPost("ordens/{id:guid}/propor-solucao")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> ProporSolucao(Guid id, PropostaSolucaoRequest request, CancellationToken ct)
        => Ok(await atendimentoService.ProporSolucaoAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));

    /// <summary>Resposta do solicitante à proposta. Só o solicitante da própria solicitação.</summary>
    /// <response code="200">Ordem atualizada.</response>
    /// <response code="400">Fora do estado AGUARDA_VALIDACAO ou motivo em falta na recusa.</response>
    /// <response code="403">Não é o solicitante.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPost("ordens/{id:guid}/responder-validacao")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> ResponderValidacao(Guid id, ResponderValidacaoRequest request, CancellationToken ct)
        => Ok(await atendimentoService.ResponderValidacaoAsync(User.GetUserId(), id, request, ct));

    /// <summary>Guarda a solução sugerida pelo assistente na ordem.</summary>
    /// <response code="200">Ordem atualizada.</response>
    /// <response code="400">Solução em falta.</response>
    /// <response code="403">Não é o técnico da ordem.</response>
    /// <response code="404">Ordem não encontrada.</response>
    [HttpPost("ordens/{id:guid}/solucao-sugerida")]
    [Authorize(Roles = "TECNICO,ADMIN")]
    [ProducesResponseType(typeof(OrdemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemDto>> SolucaoSugerida(Guid id, SolucaoSugeridaRequest request, CancellationToken ct)
        => Ok(await atendimentoService.GuardarSolucaoSugeridaAsync(User.GetUserId(), User.GetRoleAsEnum(), id, request, ct));
}
