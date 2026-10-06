using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Stock;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Stock: itens, unidades físicas e movimentos.</summary>
[ApiController]
[Route("api/stock")]
[Tags("Stock")]
public class StockController(IStockService stockService) : ControllerBase
{
    /// <summary>Lista todos os itens de stock (1 por modelo de componente).</summary>
    /// <response code="200">Lista de itens.</response>
    [HttpGet("itens")]
    [ProducesResponseType(typeof(List<ItemStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemStockDto>>> GetItens(CancellationToken ct)
        => Ok(await stockService.GetItensAsync(ct));

    /// <summary>Lista todas as unidades físicas de stock.</summary>
    /// <response code="200">Lista de unidades.</response>
    [HttpGet("unidades")]
    [ProducesResponseType(typeof(List<UnidadeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UnidadeDto>>> GetUnidades(CancellationToken ct)
        => Ok(await stockService.GetUnidadesAsync(ct));

    /// <summary>Lista todos os movimentos de stock, mais recentes primeiro.</summary>
    /// <response code="200">Lista de movimentos.</response>
    [HttpGet("movimentos")]
    [ProducesResponseType(typeof(List<MovimentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovimentoDto>>> GetMovimentos(CancellationToken ct)
        => Ok(await stockService.GetMovimentosAsync(ct));

    /// <summary>Lista paginada de unidades. Pesquisa em código e componente; filtro por estado.</summary>
    /// <response code="200">Uma página de unidades + total.</response>
    /// <response code="400">Paginação fora dos limites ou estado inválido.</response>
    [HttpGet("unidades/paginado")]
    [ProducesResponseType(typeof(PaginaResultado<UnidadeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<UnidadeDto>>> GetUnidadesPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? estado, CancellationToken ct)
        => Ok(await stockService.GetUnidadesPaginaAsync(paginacao, estado, ct));

    /// <summary>
    /// Lista paginada de movimentos, mais recentes primeiro. Pesquisa em componente e observação;
    /// filtro por tipo.
    /// </summary>
    /// <response code="200">Uma página de movimentos + total.</response>
    /// <response code="400">Paginação fora dos limites ou tipo inválido.</response>
    [HttpGet("movimentos/paginado")]
    [ProducesResponseType(typeof(PaginaResultado<MovimentoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResultado<MovimentoDto>>> GetMovimentosPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? tipo, CancellationToken ct)
        => Ok(await stockService.GetMovimentosPaginaAsync(paginacao, tipo, ct));

    /// <summary>Entrada manual de stock (ADMIN/TECNICO): cria N unidades DISPONIVEL + 1 movimento ENTRADA.</summary>
    /// <response code="201">Array com as unidades criadas.</response>
    /// <response code="400">Quantidade inválida.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Modelo de componente não encontrado.</response>
    [HttpPost("entrada")]
    [Authorize(Roles = "ADMIN,TECNICO")]
    [ProducesResponseType(typeof(List<UnidadeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<UnidadeDto>>> Entrada(EntradaStockRequest request, CancellationToken ct)
    {
        var criadas = await stockService.EntradaAsync(request, ct);
        return CreatedAtAction(nameof(GetUnidades), new { }, criadas);
    }
}
