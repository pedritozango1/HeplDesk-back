using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Novati.API.Controllers;

/// <summary>Verificação de saúde da API.</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HelphController : ControllerBase
{
    /// <summary>Confirma que a API está operacional.</summary>
    /// <response code="200">API operacional.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "ok" });
}
