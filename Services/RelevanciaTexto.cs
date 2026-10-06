using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Novati.API.Models.Entities;

namespace Novati.API.Services;

/// <summary>
/// Pontuação de relevância por termos (sem acentos, minúsculas). Partilhada pela
/// pesquisa da base de conhecimento e pelo assistente.
/// </summary>
public static partial class RelevanciaTexto
{
    // Pesos por campo do artigo: título > tags > conteúdo.
    private const int PesoTitulo = 3;
    private const int PesoTags = 2;
    private const int PesoConteudo = 1;

    [GeneratedRegex(@"[a-z0-9]+")]
    private static partial Regex TokenRegex();

    // Palavras sem valor de pesquisa (já sem acentos) — senão "nao", "com", "para"
    // aproximavam problemas que nada têm a ver.
    private static readonly HashSet<string> PalavrasVazias =
    [
        "nao", "sim", "com", "sem", "para", "por", "pelo", "pela", "que", "uma", "uns", "umas",
        "dos", "das", "nos", "nas", "num", "numa", "este", "esta", "isto", "esse", "essa", "isso",
        "ele", "ela", "eles", "elas", "meu", "minha", "seu", "sua", "tem", "ter", "foi", "ser",
        "estou", "fica", "ficou", "mais", "muito", "quando", "como", "onde", "ja", "ate",
        "mas", "tambem", "nada", "tudo", "vez", "vezes", "desde", "hoje", "ainda", "entre",
    ];

    /// <summary>Minúsculas, sem acentos, só tokens com mais de 2 caracteres e sem palavras vazias.</summary>
    public static List<string> Tokenizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return [];

        var normalizado = RemoverAcentos(texto).ToLowerInvariant();
        return [.. TokenRegex().Matches(normalizado).Select(m => m.Value)
            .Where(t => t.Length > 2 && !PalavrasVazias.Contains(t))];
    }

    public static int PontuarArtigo(Artigo artigo, IReadOnlyCollection<string> termos)
    {
        var titulo = Tokenizar(artigo.Titulo).ToHashSet();
        var tags = artigo.Tags.SelectMany(Tokenizar).ToHashSet();
        var conteudo = Tokenizar(artigo.Conteudo).ToHashSet();

        var pontos = 0;
        foreach (var termo in termos)
        {
            if (titulo.Contains(termo)) pontos += PesoTitulo;
            if (tags.Contains(termo)) pontos += PesoTags;
            if (conteudo.Contains(termo)) pontos += PesoConteudo;
        }
        return pontos;
    }

    /// <summary>Quantos termos distintos aparecem num texto livre (ex.: título + descrição de outro pedido).</summary>
    public static int PontuarTexto(string? texto, IReadOnlyCollection<string> termos)
    {
        var tokens = Tokenizar(texto).ToHashSet();
        return termos.Count(tokens.Contains);
    }

    private static string RemoverAcentos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);

        foreach (var c in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
