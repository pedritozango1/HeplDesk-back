namespace Novati.API.Common.Exceptions;

/// <summary>Conflito (ex.: duplicado). Traduzido para HTTP 409.</summary>
public class ConflictException : Exception
{
    /// <summary>Código estável para o front reagir sem comparar a frase (opcional).</summary>
    public string? Codigo { get; }

    public ConflictException(string message, string? codigo = null) : base(message) => Codigo = codigo;
}
