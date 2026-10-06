namespace Novati.API.Common.Exceptions;

/// <summary>Conflito (ex.: duplicado). Traduzido para HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
