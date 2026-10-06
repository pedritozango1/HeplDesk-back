using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Novati.API.Common.Extensions;
using Novati.API.Dtos.Ficheiros;
using Novati.API.Services.Interfaces;

namespace Novati.API.Controllers;

/// <summary>Upload e download de ficheiros (assinaturas, anexos, ...).</summary>
[ApiController]
[Route("api/ficheiros")]
[Tags("Ficheiros")]
public class FicheirosController(IFicheiroService ficheiroService) : ControllerBase
{
    // Teto do pedido HTTP um pouco acima do limite de negócio (Storage:MaxBytes, 10 MB), para o
    // excesso chegar ao Service e receber uma mensagem clara em vez de um 413 sem corpo.
    private const long LimitePedido = 12 * 1024 * 1024;

    /// <summary>Carrega um ficheiro (multipart/form-data, campo "ficheiro"). Máx. 10 MB; PNG, JPEG, GIF, WEBP, PDF, DOCX, XLSX, TXT.</summary>
    /// <response code="201">Ficheiro guardado.</response>
    /// <response code="400">Vazio, demasiado grande ou tipo não suportado.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(LimitePedido)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimitePedido)]
    [ProducesResponseType(typeof(FicheiroDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FicheiroDto>> Upload([FromForm] UploadFicheiroRequest request, CancellationToken ct)
    {
        var dto = await ficheiroService.UploadAsync(User.GetUserId(), request.Ficheiro, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>Metadados de um ficheiro.</summary>
    /// <response code="200">Metadados.</response>
    /// <response code="404">Ficheiro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FicheiroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FicheiroDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await ficheiroService.GetByIdAsync(id, ct));

    /// <summary>Conteúdo (bytes) do ficheiro.</summary>
    /// <response code="200">Conteúdo do ficheiro.</response>
    /// <response code="404">Ficheiro não encontrado.</response>
    [HttpGet("{id:guid}/conteudo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Conteudo(Guid id, CancellationToken ct)
    {
        var (conteudo, contentType, nome) = await ficheiroService.AbrirAsync(id, ct);

        // O conteúdo de um id nunca muda → o browser pode guardá-lo em cache (só para este utilizador).
        Response.Headers[HeaderNames.CacheControl] = "private, max-age=31536000, immutable";
        // Impede o browser de "adivinhar" outro tipo (ex.: tratar um .txt como HTML).
        Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";

        return File(conteudo, contentType, nome, enableRangeProcessing: true);
    }

    /// <summary>Apaga um ficheiro que já não esteja em uso (só quem o carregou ou um ADMIN).</summary>
    /// <response code="204">Ficheiro apagado.</response>
    /// <response code="403">Sem permissão.</response>
    /// <response code="404">Ficheiro não encontrado.</response>
    /// <response code="409">O ficheiro está em uso.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await ficheiroService.DeleteAsync(User.GetUserId(), User.GetRoleAsEnum(), id, ct);
        return NoContent();
    }
}
