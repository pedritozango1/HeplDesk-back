using System.Text.RegularExpressions;
using Novati.API.Common.Exceptions;
using Novati.API.Dtos.Assistente;
using Novati.API.Mappers;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Services.Assistente;

/// <summary>
/// Assistente de resolução da Base de Conhecimento. Junta o pedido com os artigos
/// (e as suas avaliações) e as reparações já resolvidas, usa o MotorSugestoes para
/// ordenar e, enquanto houver dúvida entre candidatos, faz perguntas de afinação.
/// Tudo calculado no backend com os dados da BD — sem serviços externos.
/// </summary>
public partial class AssistenteService(
    ISolicitacaoRepository solicitacoes,
    IOrdemRepository ordens,
    IArtigoRepository artigosRepo,
    IDispositivoRepository dispositivos,
    IModeloDispositivoRepository modelos)
{
    private const int MaxPerguntas = 3;
    private const double PesoResposta = 2.0;   // a resposta escolhida pesa o dobro do texto do pedido

    private record Contexto(string Titulo, string Descricao, string Categoria, string Equipamento, string? Diagnostico, bool ParaTecnico);

    public async Task<AssistenteRespostaDto> AnalisarAsync(Guid userId, Role role, AnalisarRequest req, CancellationToken ct)
    {
        var (contexto, solicitacaoId) = await CarregarContextoAsync(userId, role, req, ct);

        // Documentos: artigos (com avaliações) + reparações resolvidas de outros pedidos.
        var docs = new List<Documento>();
        docs.AddRange((await artigosRepo.GetTodosComAvaliacoesAsync(ct)).Select(MotorSugestoes.DeArtigo));
        docs.AddRange((await ordens.GetTodasCompletasAsync(null, ct))
            .Where(o => o.Estado == EstadoOrdem.RESOLVIDO && o.SolicitacaoId != solicitacaoId && !string.IsNullOrWhiteSpace(o.Solucao))
            .Select(o => MotorSugestoes.DeCaso(new CasoResolvido(o.Solicitacao.Titulo, o.Diagnostico, o.Solucao!, o.Solicitacao.Categoria))));

        // Consulta: texto do pedido + respostas escolhidas; "Nenhuma destas" penaliza as opções rejeitadas.
        var consulta = new Dictionary<string, double>();
        void Juntar(string? texto, double peso)
        {
            foreach (var t in RelevanciaTexto.Tokenizar(texto)) consulta[t] = Math.Max(consulta.GetValueOrDefault(t), peso);
        }
        Juntar(contexto.Titulo, 1);
        Juntar(contexto.Descricao, 1);
        Juntar(contexto.Diagnostico, 1);

        var penalizados = new HashSet<string>();
        var jaOferecidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in req.Respostas)
        {
            foreach (var o in r.Opcoes) jaOferecidas.Add(o);
            if (r.Resposta == MotorSugestoes.NenhumaDestas)
            {
                foreach (var o in r.Opcoes.Where(o => o != MotorSugestoes.NenhumaDestas))
                    foreach (var t in RelevanciaTexto.Tokenizar(o))
                        if (!consulta.ContainsKey(t)) penalizados.Add(t);
            }
            else
            {
                Juntar(r.Resposta, PesoResposta);
            }
        }

        var candidatos = MotorSugestoes.Ordenar(docs, consulta, contexto.Categoria, penalizados);

        var podePerguntar = !req.ForcarPlano && req.Respostas.Count < MaxPerguntas;
        if (podePerguntar)
        {
            var pergunta = MotorSugestoes.GerarPergunta(candidatos, jaOferecidas, consulta.Keys.ToHashSet());
            if (pergunta is not null)
                return new AssistenteRespostaDto("pergunta", "motor", pergunta, null);
        }

        return new AssistenteRespostaDto("plano", "motor", null, MontarPlano(contexto, candidatos));
    }

    // ── Contexto e permissões ──────────────────────────────

    private async Task<(Contexto, Guid)> CarregarContextoAsync(Guid userId, Role role, AnalisarRequest req, CancellationToken ct)
    {
        if (req.OrdemId is { } ordemId)
        {
            var ordem = await ordens.GetCompletaAsync(ordemId, ct) ?? throw new NotFoundException("Ordem não encontrada.");
            if (ordem.TecnicoId != userId && role != Role.ADMIN)
                throw new NotFoundException("Ordem não encontrada.");
            var s = ordem.Solicitacao;
            return (new Contexto(s.Titulo, s.Descricao, s.Categoria, await EquipamentoAsync(s.DispositivoFisicoId, ct),
                string.IsNullOrWhiteSpace(ordem.Diagnostico) ? null : ordem.Diagnostico, ParaTecnico: true), s.Id);
        }

        if (req.SolicitacaoId is { } solicitacaoId)
        {
            var s = await solicitacoes.GetByIdAsync(solicitacaoId, ct) ?? throw new NotFoundException("Solicitação não encontrada.");
            // 404 e não 403: não revela pedidos de outras pessoas.
            if (s.SolicitanteId != userId && role is not (Role.ADMIN or Role.GESTOR or Role.TECNICO))
                throw new NotFoundException("Solicitação não encontrada.");
            return (new Contexto(s.Titulo, s.Descricao, s.Categoria, await EquipamentoAsync(s.DispositivoFisicoId, ct),
                null, ParaTecnico: role != Role.FUNCIONARIO), s.Id);
        }

        throw new BusinessRuleException("Indique a solicitação ou a ordem a analisar.");
    }

    private async Task<string> EquipamentoAsync(Guid? dispositivoId, CancellationToken ct)
    {
        if (dispositivoId is not { } id) return "não indicado";
        var d = await dispositivos.GetByIdAsync(id, ct);
        if (d is null) return "não indicado";
        var m = await modelos.GetByIdAsync(d.ModeloDispositivoId, ct);
        return m is null ? d.Patrimonio : $"{d.Patrimonio} — {m.Nome} ({m.Tipo})";
    }

    // ── Plano ─────────────────────────────────────────────

    private static PlanoAssistenteDto MontarPlano(Contexto c, List<Candidato> candidatos)
    {
        var artigos = candidatos.Where(x => x.Doc.Tipo == TipoDocumento.Artigo).Select(x => x.Doc.Artigo!).Take(3).ToList();
        var casos = candidatos.Where(x => x.Doc.Tipo == TipoDocumento.Caso).Select(x => x.Doc.Caso!).Take(3).ToList();
        var melhorArtigo = artigos.FirstOrDefault();

        var causas = new List<string>();
        if (!string.IsNullOrWhiteSpace(c.Diagnostico)) causas.Add($"Diagnóstico registado: {c.Diagnostico}");
        causas.AddRange(casos.Select(k => $"Em «{k.Titulo}» a causa foi: {k.Diagnostico}"));
        causas.AddRange(artigos.Select(a => $"Documentado na base: «{a.Titulo}»"));

        var passos = melhorArtigo is null ? [] : Frases(melhorArtigo.Conteudo);
        // O técnico vê também como foram resolvidas as reparações semelhantes.
        if (c.ParaTecnico || passos.Count == 0)
            passos.AddRange(casos.Select(k => $"Reparação semelhante «{k.Titulo}»: {k.Solucao}"));

        var solucao = casos.FirstOrDefault()?.Solucao
            ?? (melhorArtigo is not null ? $"Aplicar o procedimento «{melhorArtigo.Titulo}» da base de conhecimento." : "");

        var resumo = c.Diagnostico is not null
            ? $"{c.Titulo} — diagnóstico: {c.Diagnostico}."
            : $"{c.Titulo} — {c.Descricao}";
        resumo += artigos.Count + casos.Count == 0
            ? " Não há artigos nem reparações semelhantes na base de conhecimento — a equipa de TI vai analisar."
            : $" Sugestões com base em {Plural(artigos.Count, "artigo", "artigos")} e {Plural(casos.Count, "reparação semelhante", "reparações semelhantes")}.";

        return new PlanoAssistenteDto(resumo, causas.Take(4).ToList(), passos.Take(8).ToList(), solucao,
            artigos.Select(ArtigoMapper.ToDto).ToList());
    }

    private static string Plural(int n, string um, string varios) => $"{n} {(n == 1 ? um : varios)}";

    [GeneratedRegex(@"\r?\n|(?<=\.)\s+(?=Passo\s*\d)")]
    private static partial Regex SeparadorPassos();

    [GeneratedRegex(@"^Passo\s*\d+\s*[:.]\s*", RegexOptions.IgnoreCase)]
    private static partial Regex PrefixoPasso();

    // "Passo 1: …. Passo 2: …" → ["…", "…"]
    private static List<string> Frases(string conteudo)
        => SeparadorPassos().Split(conteudo)
            .Select(f => PrefixoPasso().Replace(f.Trim(), ""))
            .Where(f => f.Length > 0)
            .ToList();
}
