using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Novati.API.Models.Entities;
using Novati.API.Realtime;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Data;

/// <summary>
/// Implementa IUnitOfWork sobre o AppDbContext partilhado. Depois de cada gravação
/// bem-sucedida avisa os browsers ligados (SignalR) de que recursos mudaram — é
/// isto que torna toda a app em tempo real sem código de eventos em cada service.
/// </summary>
public class UnitOfWork(
    AppDbContext ctx,
    IHubContext<TempoRealHub> hub,
    IHubContext<AcessoRemotoHub> hubAcessoRemoto,
    IHubContext<AgenteHub> hubAgentes,
    AgentesLigados agentes,
    ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Calculado antes de gravar: depois do SaveChanges as entradas ficam Unchanged.
        var recursos = RecursosAlterados.Pendentes(ctx.ChangeTracker);

        // Sessões remotas que esta gravação leva a um estado final, seja qual for o caminho
        // (uma das partes, tempo máximo, ordem reatribuída…): os dois browsers têm de desligar
        // e o agente, se houver, tem de parar.
        var sessoesFechadas = ctx.ChangeTracker.Entries<SessaoRemota>()
            .Where(e => e.State == EntityState.Modified && !e.Entity.EmCurso)
            .Select(e => new { sessaoId = e.Entity.Id, motivo = e.Entity.MotivoFim })
            .ToList();

        var linhas = await ctx.SaveChangesAsync(ct);

        // Primeiro o que não pode falhar: a partir daqui o agente já não recebe rato nem teclado
        // desta sessão, mesmo que o aviso abaixo se perca.
        var agentesAParar = sessoesFechadas
            .Select(s => (agente: agentes.Libertar(s.sessaoId), s.motivo))
            .Where(x => x.agente is not null)
            .ToList();

        if (recursos.Length > 0)
        {
            try
            {
                // Só nomes de recursos, nunca dados: cada browser recarrega pela API,
                // que aplica as regras de visibilidade do perfil.
                await hub.Clients.All.SendAsync("alterado", recursos, CancellationToken.None);

                foreach (var fechada in sessoesFechadas)
                    await hubAcessoRemoto.Clients.Group(AcessoRemotoHub.GrupoSessao(fechada.sessaoId))
                        .SendAsync("terminada", fechada, CancellationToken.None);

                foreach (var (agente, motivo) in agentesAParar)
                    await hubAgentes.Clients.Client(agente!.LigacaoId)
                        .SendAsync("parar", motivo ?? "Sessão terminada.", CancellationToken.None);
            }
            catch (Exception ex)
            {
                // A gravação já foi feita; um aviso em tempo real perdido não a desfaz.
                logger.LogWarning(ex, "Falha ao avisar clientes em tempo real ({Recursos})", string.Join(",", recursos));
            }
        }
        return linhas;
    }
}
