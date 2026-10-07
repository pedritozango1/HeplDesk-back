namespace Novati.API.Common.Exceptions;

/// <summary>Limite de tentativas atingido. Traduzido para HTTP 429.</summary>
public class DemasiadasTentativasException : Exception
{
    /// <summary>Código estável para o front reagir sem comparar a frase (opcional).</summary>
    public string? Codigo { get; }

    public DemasiadasTentativasException(string message, string? codigo = null) : base(message) => Codigo = codigo;
}
