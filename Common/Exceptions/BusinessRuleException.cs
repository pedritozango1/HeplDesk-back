namespace Novati.API.Common.Exceptions;

/// <summary>Regra de negócio violada. Traduzido para HTTP 400.</summary>
public class BusinessRuleException : Exception
{
    /// <summary>Código estável para o front reagir sem comparar a frase (opcional).</summary>
    public string? Codigo { get; }

    public BusinessRuleException(string message, string? codigo = null) : base(message) => Codigo = codigo;
}
