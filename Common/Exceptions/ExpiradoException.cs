namespace Novati.API.Common.Exceptions;

/// <summary>O recurso existiu mas já caducou (ex.: token fora de prazo). Traduzido para HTTP 410.</summary>
public class ExpiradoException : Exception
{
    /// <summary>Código estável para o front reagir sem comparar a frase (opcional).</summary>
    public string? Codigo { get; }

    public ExpiradoException(string message, string? codigo = null) : base(message) => Codigo = codigo;
}
