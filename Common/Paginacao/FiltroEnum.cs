using Novati.API.Common.Exceptions;

namespace Novati.API.Common.Paginacao;

/// <summary>Lê um filtro de enum vindo da query string (?estado=ATIVO).</summary>
public static class FiltroEnum
{
    /// <summary>Vazio → null (sem filtro). Valor que não existe no enum → HTTP 400.</summary>
    public static TEnum? Ler<TEnum>(string? valor, string nome) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        // IsDefined: o TryParse sozinho aceitaria números ("7") que não são membros do enum.
        if (!Enum.TryParse<TEnum>(valor, out var resultado) || !Enum.IsDefined(resultado))
            throw new BusinessRuleException($"Filtro '{nome}' inválido.");

        return resultado;
    }
}
