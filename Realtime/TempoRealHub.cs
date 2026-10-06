using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Novati.API.Common.Extensions;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Realtime;

/// <summary>
/// Canal em tempo real (SignalR, /hubs/tempo-real) com o mesmo JWT da API.
/// Eventos enviados ao browser:
///   "alterado"  (string[] recursos)         — dados que mudaram; o front recarrega-os da API
///   "presenca"  (Guid[] online)              — quem está ligado
///   "aEscrever" ({ solicitacaoId, userId }) — alguém está a escrever numa conversa
/// </summary>
[Authorize]
public class TempoRealHub(
    PresencaTracker presenca,
    ISolicitacaoRepository solicitacoes,
    IOrdemRepository ordens) : Hub
{
    public static string GrupoUser(Guid id) => $"user:{id}";
    public static string GrupoRole(Role role) => $"role:{role}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoUser(userId));
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoRole(Context.User!.GetRoleAsEnum()));

        presenca.Ligou(userId);
        // Todos recebem a lista atualizada (inclui quem acabou de ligar, que precisa do estado inicial).
        await Clients.All.SendAsync("presenca", presenca.Online);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (presenca.Desligou(Context.User!.GetUserId()))
            await Clients.All.SendAsync("presenca", presenca.Online);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// "Estou a escrever" numa conversa: só chega a quem participa nela
    /// (solicitante, técnico da ordem, gestores e administradores).
    /// </summary>
    public async Task AEscrever(Guid solicitacaoId)
    {
        var userId = Context.User!.GetUserId();
        var role = Context.User!.GetRoleAsEnum();

        var solicitacao = await solicitacoes.GetByIdAsync(solicitacaoId);
        if (solicitacao is null) return;
        var tecnicoId = await ordens.GetTecnicoIdPorSolicitacaoAsync(solicitacaoId);

        // Mesmas regras de acesso do chat (ChatService).
        var participa = role is Role.ADMIN or Role.GESTOR
            || solicitacao.SolicitanteId == userId
            || tecnicoId == userId;
        if (!participa) return;

        var grupos = new List<string> { GrupoUser(solicitacao.SolicitanteId), GrupoRole(Role.ADMIN), GrupoRole(Role.GESTOR) };
        if (tecnicoId is { } t) grupos.Add(GrupoUser(t));

        await Clients.Groups(grupos).SendAsync("aEscrever", new { solicitacaoId, userId });
    }
}
