namespace Novati.API.Common.Exceptions;

/// <summary>Regra de negócio violada. Traduzido para HTTP 400.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}