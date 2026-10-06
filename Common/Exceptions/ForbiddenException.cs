namespace Novati.API.Common.Exceptions;

/// <summary>Ação não permitida para o utilizador atual. Traduzido para HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
