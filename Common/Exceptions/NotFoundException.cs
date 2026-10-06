namespace Novati.API.Common.Exceptions;

/// <summary>Recurso não encontrado. Traduzido para HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
