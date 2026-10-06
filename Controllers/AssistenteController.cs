using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.Assistente;
using Novati.API.Services.Assistente;

namespace Novati.API.Controllers;

/// <summary>Assistente de resolução com IA (Claude) sobre a base de conhecimento.</summary>
[ApiController]
[Route("api/assistente")]
[Tags("Assistente")]
public class AssistenteController(AssistenteService assistente) : ControllerBase
{
    /// <summary>
    /// Analisa uma solicitação (funcionário) ou ordem (técnico) e devolve a próxima
    /// pergunta de afinação ou o plano de resolução.
    /// </summary>
    /// <response code="200">Pergunta (tipo "pergunta") ou plano (tipo "plano"); origem "ia" ou "base".</response>
    /// <response code="400">Nem solicitação nem ordem indicadas.</response>
    /// <response code="404">Pedido inexistente ou sem acesso.</response>
    [HttpPost("analisar")]
    [ProducesResponseType(typeof(AssistenteRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssistenteRespostaDto>> Analisar(AnalisarRequest request, CancellationToken ct)
        => Ok(await assistente.AnalisarAsync(User.GetUserId(), User.GetRoleAsEnum(), request, ct));
}
