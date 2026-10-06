using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Dtos.Dominio;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Valores de domínio (enums com rótulos, categorias, tipos, SLA) para o front.</summary>
[ApiController]
[Route("api/dominio")]
[Tags("Domínio")]
public class DominioController(IDominioService dominio) : ControllerBase
{
    /// <summary>
    /// Devolve os valores de domínio. Anónimo: o ecrã de login também precisa dos
    /// rótulos, e nada disto é informação sensível.
    /// </summary>
    /// <response code="200">Enums com rótulo e cor, categorias, tipos e horas de SLA.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DominioDto), StatusCodes.Status200OK)]
    public ActionResult<DominioDto> Get() => Ok(dominio.Obter());
}
