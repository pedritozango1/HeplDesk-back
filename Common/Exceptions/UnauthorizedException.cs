namespace Novati.API.Common.Exceptions;

/// <summary>Credenciais inválidas. Traduzido para HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}