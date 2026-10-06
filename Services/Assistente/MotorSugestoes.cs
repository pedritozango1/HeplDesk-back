using Novati.API.Dtos.Assistente;
using Novati.API.Models.Entities;

namespace Novati.API.Services.Assistente;

/// <summary>Uma reparação já resolvida, usada como "caso semelhante".</summary>
public record CasoResolvido(string Titulo, string Diagnostico, string Solucao, string Categoria);

public enum TipoDocumento { Artigo, Caso }

/// <summary>
/// Um documento pesquisável: um artigo da base de conhecimento ou uma reparação resolvida.
/// Frequencias = termos com o peso do campo onde aparecem (título conta mais que conteúdo).
/// Reforco = multiplicador vindo das avaliações "Isto ajudou?" (1 = neutro).
/// </summary>
public record Documento(
    TipoDocumento Tipo,
    string Titulo,
    string Categoria,
    IReadOnlyList<string> Etiquetas,
    IReadOnlyDictionary<string, double> Frequencias,
    double Comprimento,
    double Reforco,
    Artigo? Artigo,
    CasoResolvido? Caso);

public record Candidato(Documento Doc, double Pontos);

/// <summary>
/// Motor de sugestões próprio (sem serviços externos):
///  - ordena artigos e reparações resolvidas por BM25 (palavras raras pesam mais);
///  - multiplica pelo saldo de avaliações "Isto ajudou?" e dá um bónus à mesma categoria;
///  - quando os melhores candidatos estão empatados, gera uma pergunta de afinação
///    com opções tiradas dos próprios dados (tags dos artigos ou títulos dos candidatos).
/// </summary>
public static class MotorSugestoes
{
    public const string NenhumaDestas = "Nenhuma destas";

    // Parâmetros clássicos do BM25.
    private const double K1 = 1.2;
    private const double B = 0.75;

    // Pesos por campo.
    private const double PesoTitulo = 3, PesoTags = 2, PesoDiagnostico = 2, PesoCategoria = 1, PesoTexto = 1;

    private const double BonusMesmaCategoria = 1.15;
    private const double PenalizacaoRejeitado = 0.3;   // candidatos ligados a opções rejeitadas ("Nenhuma destas")
    private const double FracaoMinima = 0.35;          // só candidatos com ≥35% da pontuação do melhor
    private const int MaxCandidatos = 6;
    private const double VantagemClara = 1.8;          // melhor ≥ 1,8 × segundo → não é preciso perguntar
    private const int MaxOpcoes = 3;
    private const int MinTermosComuns = 2;           // uma palavra solta ("liga") não chega para ser relevante

    // ── Documentos ────────────────────────────────────────

    public static Documento DeArtigo(Artigo a)
    {
        var f = new Dictionary<string, double>();
        Somar(f, a.Titulo, PesoTitulo);
        foreach (var t in a.Tags) Somar(f, t, PesoTags);
        Somar(f, a.Categoria, PesoCategoria);
        Somar(f, a.Conteudo, PesoTexto);

        // (úteis + 1) / (total + 2) ∈ ]0,1[ — 0,5 sem avaliações → reforço 1 (neutro); vai de 0,6 a 1,4.
        var uteis = a.Avaliacoes.Count(x => x.Util);
        var total = a.Avaliacoes.Count;
        var reforco = 0.6 + 0.8 * (uteis + 1.0) / (total + 2.0);

        return new Documento(TipoDocumento.Artigo, a.Titulo, a.Categoria, a.Tags, f, f.Values.Sum(), reforco, a, null);
    }

    public static Documento DeCaso(CasoResolvido c)
    {
        var f = new Dictionary<string, double>();
        Somar(f, c.Titulo, PesoTitulo);
        Somar(f, c.Diagnostico, PesoDiagnostico);
        Somar(f, c.Solucao, PesoTexto);
        return new Documento(TipoDocumento.Caso, c.Titulo, c.Categoria, [], f, f.Values.Sum(), 1.0, null, c);
    }

    private static void Somar(Dictionary<string, double> f, string? texto, double peso)
    {
        foreach (var t in RelevanciaTexto.Tokenizar(texto))
            f[t] = f.GetValueOrDefault(t) + peso;
    }

    // ── Ordenação ─────────────────────────────────────────

    /// <param name="docs">Artigos e reparações resolvidas.</param>
    /// <param name="consulta">Termo → peso (respostas escolhidas pesam mais que o texto do pedido).</param>
    /// <param name="categoria">Categoria do pedido (bónus aos documentos da mesma categoria).</param>
    /// <param name="penalizados">Termos das opções rejeitadas com "Nenhuma destas".</param>
    public static List<Candidato> Ordenar(
        IReadOnlyList<Documento> docs,
        IReadOnlyDictionary<string, double> consulta,
        string categoria,
        IReadOnlySet<string> penalizados)
    {
        if (docs.Count == 0 || consulta.Count == 0) return [];

        var mediaComprimento = docs.Average(d => d.Comprimento);
        var n = docs.Count;
        // Em quantos documentos aparece cada termo da consulta.
        var df = consulta.Keys.ToDictionary(t => t, t => docs.Count(d => d.Frequencias.ContainsKey(t)));

        var minComuns = Math.Min(MinTermosComuns, consulta.Count);
        var pontuados = new List<Candidato>();
        foreach (var d in docs)
        {
            double pontos = 0;
            var comuns = 0;
            foreach (var (termo, pesoConsulta) in consulta)
            {
                if (!d.Frequencias.TryGetValue(termo, out var tf)) continue;
                comuns++;
                var idf = Math.Log(1 + (n - df[termo] + 0.5) / (df[termo] + 0.5));
                var norma = tf * (K1 + 1) / (tf + K1 * (1 - B + B * d.Comprimento / mediaComprimento));
                pontos += pesoConsulta * idf * norma;
            }
            if (pontos <= 0 || comuns < minComuns) continue;

            pontos *= d.Reforco;
            if (!string.IsNullOrWhiteSpace(categoria) && string.Equals(d.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
                pontos *= BonusMesmaCategoria;
            if (penalizados.Count > 0 && d.Frequencias.Keys.Any(penalizados.Contains))
                pontos *= PenalizacaoRejeitado;

            pontuados.Add(new Candidato(d, pontos));
        }

        if (pontuados.Count == 0) return [];
        var melhor = pontuados.Max(c => c.Pontos);
        return pontuados
            .Where(c => c.Pontos >= melhor * FracaoMinima)
            .OrderByDescending(c => c.Pontos)
            .Take(MaxCandidatos)
            .ToList();
    }

    // ── Pergunta de afinação ──────────────────────────────

    /// <summary>
    /// Pergunta que separa os melhores candidatos, ou null se um deles já se destaca
    /// (ou se não há nada que os distinga). As opções saem dos dados, nunca de uma lista fixa.
    /// </summary>
    public static PerguntaAssistenteDto? GerarPergunta(
        IReadOnlyList<Candidato> candidatos,
        IReadOnlySet<string> jaOferecidas,
        IReadOnlySet<string> termosConsulta)
    {
        var topo = candidatos.Take(5).ToList();
        if (topo.Count < 2) return null;
        if (topo[0].Pontos >= VantagemClara * topo[1].Pontos) return null;

        // Estratégia 1: tags dos artigos que só alguns candidatos têm.
        var cobertura = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < topo.Count; i++)
            foreach (var tag in topo[i].Doc.Etiquetas)
            {
                if (!cobertura.TryGetValue(tag, out var set)) cobertura[tag] = set = [];
                set.Add(i);
            }

        var informativas = cobertura
            .Where(kv => kv.Value.Count < topo.Count                                   // distingue
                && !jaOferecidas.Contains(kv.Key)                                     // não repete
                && !RelevanciaTexto.Tokenizar(kv.Key).All(termosConsulta.Contains))   // não é o que já se sabe
            .ToList();

        // Uma opção por candidato, do melhor para o pior, escolhendo a tag mais específica dele.
        var opcoes = new List<string>();
        for (var i = 0; i < topo.Count && opcoes.Count < MaxOpcoes; i++)
        {
            var tag = informativas
                .Where(kv => kv.Value.Contains(i) && !opcoes.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Value.Count)
                .Select(kv => kv.Key)
                .FirstOrDefault();
            if (tag is not null) opcoes.Add(tag);
        }
        if (opcoes.Count >= 2)
            return new PerguntaAssistenteDto("O problema está mais relacionado com qual destes pontos?", [.. opcoes, NenhumaDestas]);

        // Estratégia 2: os títulos dos próprios candidatos (artigos e reparações resolvidas).
        var titulos = topo
            .Select(c => c.Doc.Titulo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(t => !jaOferecidas.Contains(t))
            .Take(MaxOpcoes)
            .ToList();
        if (titulos.Count >= 2)
            return new PerguntaAssistenteDto("Qual destas situações é mais parecida com a sua?", [.. titulos, NenhumaDestas]);

        return null;
    }
}
