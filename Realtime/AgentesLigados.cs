using System.Security.Cryptography;

namespace Novati.API.Realtime;

/// <summary>
/// Agentes Novati ligados neste momento (em memória, como o PresencaTracker).
/// Um agente não tem conta nem palavra-passe: ao ligar-se recebe um código que mostra no
/// ecrã do PC. Só fica associado a uma sessão quando o solicitante, com sessão iniciada no
/// Novati, escreve esse código ao autorizar — e só transmite com a sessão ATIVA.
/// </summary>
public class AgentesLigados
{
    public sealed class Agente(string ligacaoId, string codigo, string nomePc)
    {
        public string LigacaoId { get; } = ligacaoId;
        public string Codigo { get; } = codigo;
        public string NomePc { get; } = nomePc;

        /// <summary>Sessão a que está reservado (desde a autorização do solicitante).</summary>
        public Guid? SessaoId { get; set; }

        /// <summary>True depois de o técnico resgatar o token: só então as imagens são aceites.</summary>
        public bool Ativo { get; set; }
    }

    // Trava contra abuso: o hub dos agentes é anónimo, não pode crescer sem limite.
    private const int Maximo = 2000;

    private readonly Lock trinco = new();
    private readonly Dictionary<string, Agente> porLigacao = [];

    /// <summary>Regista a ligação e devolve o código a mostrar no PC. Null se o limite foi atingido.</summary>
    public string? Registar(string ligacaoId, string nomePc)
    {
        lock (trinco)
        {
            if (porLigacao.TryGetValue(ligacaoId, out var existente))
                return existente.Codigo;
            if (porLigacao.Count >= Maximo)
                return null;

            string codigo;
            do codigo = RandomNumberGenerator.GetInt32(100_000_000, 1_000_000_000).ToString();
            while (porLigacao.Values.Any(a => a.Codigo == codigo));

            porLigacao[ligacaoId] = new Agente(ligacaoId, codigo, nomePc);
            return codigo;
        }
    }

    public Agente? PorLigacao(string ligacaoId)
    {
        lock (trinco) return porLigacao.GetValueOrDefault(ligacaoId);
    }

    public Agente? PorSessao(Guid sessaoId)
    {
        lock (trinco) return porLigacao.Values.FirstOrDefault(a => a.SessaoId == sessaoId);
    }

    /// <summary>Associa o agente com este código à sessão. Null se não existe ou já está noutra sessão.</summary>
    public Agente? Reservar(string codigo, Guid sessaoId)
    {
        lock (trinco)
        {
            var agente = porLigacao.Values.FirstOrDefault(a => a.Codigo == codigo);
            if (agente is null || agente.SessaoId is not null)
                return null;

            agente.SessaoId = sessaoId;
            return agente;
        }
    }

    /// <summary>A sessão ficou ATIVA: o agente passa a poder transmitir.</summary>
    public Agente? Ativar(Guid sessaoId)
    {
        lock (trinco)
        {
            var agente = porLigacao.Values.FirstOrDefault(a => a.SessaoId == sessaoId);
            if (agente is not null) agente.Ativo = true;
            return agente;
        }
    }

    /// <summary>A sessão acabou: o agente fica livre para outra. Devolve-o, para ser avisado.</summary>
    public Agente? Libertar(Guid sessaoId)
    {
        lock (trinco)
        {
            var agente = porLigacao.Values.FirstOrDefault(a => a.SessaoId == sessaoId);
            if (agente is null) return null;

            agente.SessaoId = null;
            agente.Ativo = false;
            return agente;
        }
    }

    public void Remover(string ligacaoId)
    {
        lock (trinco) porLigacao.Remove(ligacaoId);
    }
}
