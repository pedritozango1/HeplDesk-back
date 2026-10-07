using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Novati.API.Services.Interfaces;

namespace Novati.API.Realtime;

/// <summary>
/// Ligação dos Agentes Novati (SignalR, /hubs/agente) — o programa que corre no PC do
/// solicitante. É anónima: o agente não guarda credenciais. O que lhe dá poder é a
/// associação a uma sessão, feita pelo solicitante autenticado (ver AgentesLigados).
/// Agente → servidor:  Registar(nomePc, versao) · Quadro(jpeg, largura, altura) · Terminar()
/// Servidor → agente:  "iniciar" (tecnico) · "parar" (motivo) · "visto" · "refrescar" · "entrada" (json)
/// </summary>
[AllowAnonymous]
public class AgenteHub(
    AgentesLigados agentes,
    IHubContext<AcessoRemotoHub> visores,
    IAcessoRemotoService acessoRemoto) : Hub
{
    /// <summary>Tamanho máximo de uma imagem do ecrã. O limite de mensagem do hub (Program.cs) conta com o base64.</summary>
    public const int MaxImagemBytes = 2_500_000;

    /// <summary>O agente apresenta-se e recebe o código que vai mostrar no ecrã do PC.</summary>
    public string Registar(string nomePc, string versao)
    {
        var nome = string.IsNullOrWhiteSpace(nomePc) ? "PC" : nomePc.Trim();
        if (nome.Length > 64) nome = nome[..64];

        return agentes.Registar(Context.ConnectionId, nome)
               ?? throw new HubException("Demasiados agentes ligados. Tente mais tarde.");
    }

    /// <summary>Uma imagem do ecrã. Só é reencaminhada (ao técnico da sessão) com a sessão ATIVA.</summary>
    public async Task Quadro(byte[] jpeg, int largura, int altura)
    {
        var agente = agentes.PorLigacao(Context.ConnectionId);
        if (agente is not { Ativo: true, SessaoId: { } sessaoId })
            return;
        if (jpeg is null || jpeg.Length == 0 || jpeg.Length > MaxImagemBytes)
            return;

        await visores.Clients.Group(AcessoRemotoHub.GrupoSessao(sessaoId))
            .SendAsync("quadro", new { dados = jpeg, largura, altura });
    }

    /// <summary>O solicitante carregou em "Terminar acesso" na janela do agente.</summary>
    public async Task Terminar()
    {
        if (agentes.PorLigacao(Context.ConnectionId)?.SessaoId is { } sessaoId)
            await acessoRemoto.TerminarPeloAgenteAsync(sessaoId, "Terminada no PC do solicitante.");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Fechar o agente (ou perder a rede) é o corte de emergência: a sessão não fica aberta
        // à espera de um agente que já não está lá.
        if (agentes.PorLigacao(Context.ConnectionId)?.SessaoId is { } sessaoId)
            await acessoRemoto.TerminarPeloAgenteAsync(sessaoId, "O agente foi fechado no PC do solicitante.");

        agentes.Remover(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
