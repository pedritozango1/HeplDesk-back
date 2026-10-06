using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Dtos.Auth;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Autenticação: login e emissão de JWT.</summary>
[ApiController]
[Route("api/auth")]
[Tags("Autenticação")]
public class AuthController(IAuthService authService, IUserService userService, IWebHostEnvironment env) : ControllerBase
{
    /// <summary>Password comum dos utilizadores do seed e dos criados pelo ADMIN (UserService).</summary>
    private const string PasswordDemo = "novati123";

    /// <summary>
    /// Contas de demonstração para o ecrã de login (um clique preenche as credenciais).
    /// Só existe em Development — noutros ambientes responde 404.
    /// </summary>
    /// <response code="200">Utilizadores existentes e a password do seed.</response>
    /// <response code="404">Fora de Development.</response>
    [HttpGet("contas-demo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContasDemoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContasDemoResponse>> ContasDemo(CancellationToken ct)
    {
        if (!env.IsDevelopment())
            return NotFound(new { statusCode = 404, message = "Não disponível." });

        var contas = (await userService.GetDirectoryAsync(ct))
            .Select(u => new ContaDemoDto(u.Nome, u.Email, u.Role))
            .ToList();
        return Ok(new ContasDemoResponse(PasswordDemo, contas));
    }

    /// <summary>Autentica o utilizador e devolve um JWT.</summary>
    /// <response code="200">Login válido — devolve o token e os dados do utilizador.</response>
    /// <response code="401">Email ou password inválidos.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await authService.LoginAsync(request, ct);
        return Ok(response);
    }
}
