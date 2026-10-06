using System.Text.Json;
using Novati.API.Common.Exceptions;

namespace Novati.API.Common.Middleware;

/// <summary>
/// Middleware que apanha qualquer exceção não tratada e devolve JSON no formato
/// { statusCode, message } — o formato que o front-end espera.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)         { await WriteAsync(context, 404, ex.Message); }
        catch (ForbiddenException ex)        { await WriteAsync(context, 403, ex.Message); }
        catch (ConflictException ex)         { await WriteAsync(context, 409, ex.Message); }
        catch (BusinessRuleException ex)     { await WriteAsync(context, 400, ex.Message); }
        catch (UnauthorizedException ex)     { await WriteAsync(context, 401, ex.Message); }
        catch (Exception ex)
        {
            // Exceção desconhecida → 500 com mensagem genérica. O erro real vai para o log.
            logger.LogError(ex, "Erro não tratado");
            await WriteAsync(context, 500, "Erro interno do servidor.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new { statusCode, message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}