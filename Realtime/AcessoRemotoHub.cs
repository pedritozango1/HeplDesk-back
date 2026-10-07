using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Novati.API.Common.Extensions;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Realtime;

/// <summary>
/// O lado do browser numa sessão remota ATIVA (SignalR, /hubs/acesso-remoto), com o mesmo JWT da API.
/// O hub não decide nada: só passa mensagens entre os participantes.
///
/// Modo VER (WebRTC browser a browser) — sinalização:
///   Sinal(tipo, dados) → "sinal" ({ tipo, dados, de }) no outro participante
/// Modo CONTROLAR (Agente Novati no PC do solicitante) — retransmissão:
///   "quadro" ({ dados, largura, altura }) — imagem do ecrã vinda do agente (AgenteHub)
///   QuadroVisto()   — o técnico confirma a imagem; o agente só então envia a seguinte
///   Entrada(dados)  — rato/teclado do técnico, reencaminhado para o agente
///
/// Comuns:  Entrar(sessaoId) · "parEntrou" / "parSaiu" ({ userId }) ·
///          "terminada" ({ sessaoId, motivo }) — enviado pelo UnitOfWork ao gravar o fim.
/// </summary>
[Authorize]
public class AcessoRemotoHub(
    ISessaoRemotaRepository sessoes,
    AgentesLigados agentes,
    IHubContext<AgenteHub> hubAgentes) : Hub
{
    private const string ChaveSessao = "sessao";
    private const string ChaveControla = "controla";

    // Uma descrição SDP de partilha de ecrã tem poucos KB; isto fica abaixo dos 32 KB por mensagem do SignalR.
    private const int MaxDados = 24_000;

    // Um evento de rato ou teclado são meia dúzia de campos.
    private const int MaxEntrada = 200;

    private static readonly HashSet<string> Tipos = ["oferta", "resposta", "candidato"];

    public static string GrupoSessao(Guid id) => $"sessao:{id}";

    /// <summary>Junta esta ligação à sessão. Só os dois participantes, e só com a sessão ATIVA.</summary>
    public async Task Entrar(Guid sessaoId)
    {
        var userId = Context.User!.GetUserId();

        var sessao = await sessoes.GetByIdAsync(sessaoId);
        if (sessao is null || (sessao.TecnicoId != userId && sessao.SolicitanteId != userId))
            throw new HubException("Não participa nesta sessão de acesso remoto.");

        if (sessao.Estado != EstadoSessaoRemota.ATIVA)
            throw new HubException("Esta sessão não está ativa.");

        Context.Items[ChaveSessao] = sessaoId;
        // Só o técnico da sessão, e só no modo CONTROLAR, pode enviar rato e teclado.
        Context.Items[ChaveControla] = sessao.Modo == ModoAcessoRemoto.CONTROLAR && sessao.TecnicoId == userId;

        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoSessao(sessaoId));
        await Clients.OthersInGroup(GrupoSessao(sessaoId)).SendAsync("parEntrou", new { userId });

        // Quem acabou de entrar ainda não viu nada: pede ao agente a imagem inteira.
        if (sessao.Modo == ModoAcessoRemoto.CONTROLAR)
            await AoAgente(sessaoId, "refrescar");
    }

    /// <summary>Reencaminha uma mensagem de sinalização para o outro participante, sem a interpretar.</summary>
    public async Task Sinal(string tipo, string dados)
    {
        // Só quem passou pelo Entrar (validado contra a BD) tem a sessão guardada na ligação.
        if (Context.Items[ChaveSessao] is not Guid sessaoId)
            throw new HubException("Entre primeiro na sessão.");

        if (!Tipos.Contains(tipo))
            throw new HubException("Tipo de sinal inválido.");

        if (dados is null || dados.Length > MaxDados)
            throw new HubException("Sinal demasiado grande.");

        await Clients.OthersInGroup(GrupoSessao(sessaoId))
            .SendAsync("sinal", new { tipo, dados, de = Context.User!.GetUserId() });
    }

    /// <summary>O browser do técnico mostrou a última imagem: o agente pode enviar a seguinte.</summary>
    public async Task QuadroVisto()
    {
        if (Context.Items[ChaveControla] is true && Context.Items[ChaveSessao] is Guid sessaoId)
            await AoAgente(sessaoId, "visto");
    }

    /// <summary>Um evento de rato ou teclado do técnico, para o agente aplicar no PC.</summary>
    public async Task Entrada(string dados)
    {
        if (Context.Items[ChaveControla] is not true || Context.Items[ChaveSessao] is not Guid sessaoId)
            throw new HubException("Não pode controlar esta sessão.");

        if (dados is null || dados.Length > MaxEntrada)
            throw new HubException("Evento inválido.");

        await AoAgente(sessaoId, "entrada", dados);
    }

    // Quando a sessão termina o agente é libertado (UnitOfWork): a partir daí isto não envia nada,
    // mesmo que o browser do técnico continue ligado.
    private async Task AoAgente(Guid sessaoId, string evento, string? dados = null)
    {
        if (agentes.PorSessao(sessaoId) is not { Ativo: true } agente)
            return;

        var cliente = hubAgentes.Clients.Client(agente.LigacaoId);
        if (dados is null) await cliente.SendAsync(evento);
        else await cliente.SendAsync(evento, dados);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(ChaveSessao, out var valor) && valor is Guid sessaoId)
        {
            await Clients.OthersInGroup(GrupoSessao(sessaoId))
                .SendAsync("parSaiu", new { userId = Context.User!.GetUserId() });
        }

        await base.OnDisconnectedAsync(exception);
    }
}
