using System.Text.Json;
using Novati.API.Common.Exceptions;

namespace Novati.API.Common.Middleware;

/// <summary>
/// Middleware que apanha qualquer exceção não tratada e devolve JSON no formato
/// { statusCode, message } — o formato que o front-end espera. Algumas exceções
/// trazem ainda um "codigo" estável (ex.: TOKEN_EXPIRADO).
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)              { await WriteAsync(context, 404, ex.Message); }
        catch (ForbiddenException ex)             { await WriteAsync(context, 403, ex.Message); }
        catch (ConflictException ex)              { await WriteAsync(context, 409, ex.Message, ex.Codigo); }
        catch (BusinessRuleException ex)          { await WriteAsync(context, 400, ex.Message, ex.Codigo); }
        catch (UnauthorizedException ex)          { await WriteAsync(context, 401, ex.Message); }
        catch (ExpiradoException ex)              { await WriteAsync(context, 410, ex.Message, ex.Codigo); }
        catch (DemasiadasTentativasException ex)  { await WriteAsync(context, 429, ex.Message, ex.Codigo); }
        catch (Exception ex)
        {
            // Exceção desconhecida → 500 com mensagem genérica. O erro real vai para o log.
            logger.LogError(ex, "Erro não tratado");
            await WriteAsync(context, 500, "Erro interno do servidor.");
        }
    }

    public static async Task WriteAsync(HttpContext context, int statusCode, string message, string? codigo = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        // Sem código o JSON fica exatamente como sempre foi: { statusCode, message }.
        object payload = codigo is null
            ? new { statusCode, message }
            : new { statusCode, message, codigo };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
