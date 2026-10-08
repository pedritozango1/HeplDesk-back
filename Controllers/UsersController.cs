using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novati.API.Common.Extensions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Users;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Gestão de utilizadores.</summary>
[ApiController]
[Route("api/users")]
[Tags("Utilizadores")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Dados do utilizador autenticado.</summary>
    /// <response code="200">Dados do utilizador atual.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken ct)
    {
        var dto = await userService.GetMeAsync(User.GetUserId(), ct);
        return Ok(dto);
    }

    /// <summary>Diretório de utilizadores — visível a todos os perfis autenticados.</summary>
    /// <response code="200">Lista de utilizadores.</response>
    [HttpGet("directory")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetDirectory(CancellationToken ct)
    {
        var dtos = await userService.GetDirectoryAsync(ct);
        return Ok(dtos);
    }

    /// <summary>Lista todos os utilizadores (apenas ADMIN).</summary>
    /// <response code="200">Lista de utilizadores.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken ct)
    {
        var dtos = await userService.GetAllAsync(ct);
        return Ok(dtos);
    }

    /// <summary>Lista paginada de utilizadores (apenas ADMIN). Pesquisa em nome e email; filtro por perfil.</summary>
    /// <response code="200">Uma página de utilizadores + total.</response>
    /// <response code="400">Paginação fora dos limites ou perfil inválido.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    [HttpGet("paginado")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(PaginaResultado<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginaResultado<UserDto>>> GetPaginado(
        [FromQuery] PaginacaoQuery paginacao, [FromQuery] string? role, CancellationToken ct)
        => Ok(await userService.GetPaginaAsync(paginacao, role, ct));

    /// <summary>Cria um novo utilizador (apenas ADMIN). Password inicial: novati123.</summary>
    /// <response code="201">Utilizador criado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="409">Já existe um utilizador com este email.</response>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var dto = await userService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetMe), new { }, dto);
    }

    /// <summary>Atualiza um utilizador existente (apenas ADMIN).</summary>
    /// <response code="200">Utilizador atualizado.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Utilizador não encontrado.</response>
    /// <response code="409">Já existe outro utilizador com este email.</response>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var dto = await userService.UpdateAsync(id, request, ct);
        return Ok(dto);
    }

    /// <summary>Remove um utilizador (apenas ADMIN). Não é possível apagar-se a si próprio.</summary>
    /// <response code="204">Utilizador removido.</response>
    /// <response code="400">Tentativa de apagar o próprio utilizador.</response>
    /// <response code="403">Utilizador sem permissão.</response>
    /// <response code="404">Utilizador não encontrado.</response>
    /// <response code="409">Utilizador tem registos associados.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await userService.DeleteAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

    /// <summary>O utilizador autenticado altera o seu nome e email. O perfil de acesso só o ADMIN muda.</summary>
    /// <response code="200">Utilizador atualizado.</response>
    /// <response code="400">Nome vazio ou email inválido.</response>
    /// <response code="409">Já existe outro utilizador com este email.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateMeRequest request, CancellationToken ct)
        => Ok(await userService.UpdateMeAsync(User.GetUserId(), request, ct));

    /// <summary>
    /// O utilizador autenticado troca a sua password. A nova tem de ser forte: 8+ caracteres,
    /// com maiúscula, minúscula, número e símbolo.
    /// </summary>
    /// <response code="204">Password alterada.</response>
    /// <response code="400">Password atual errada, nova password fraca ou igual à atual.</response>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest request, CancellationToken ct)
    {
        await userService.UpdatePasswordAsync(User.GetUserId(), request, ct);
        return NoContent();
    }

    /// <summary>Atualiza a assinatura digital do próprio utilizador. <c>dataUrl: null</c> remove a assinatura.</summary>
    /// <response code="200">Utilizador atualizado.</response>
    [HttpPut("me/signature")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> UpdateSignature(SignatureRequest request, CancellationToken ct)
    {
        var dto = await userService.UpdateSignatureAsync(User.GetUserId(), request, ct);
        return Ok(dto);
    }
}
