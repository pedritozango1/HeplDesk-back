using Microsoft.AspNetCore.SignalR;
using Novati.API.Realtime;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Data;

/// <summary>
/// Implementa IUnitOfWork sobre o AppDbContext partilhado. Depois de cada gravação
/// bem-sucedida avisa os browsers ligados (SignalR) de que recursos mudaram — é
/// isto que torna toda a app em tempo real sem código de eventos em cada service.
/// </summary>
public class UnitOfWork(AppDbContext ctx, IHubContext<TempoRealHub> hub, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Calculado antes de gravar: depois do SaveChanges as entradas ficam Unchanged.
        var recursos = RecursosAlterados.Pendentes(ctx.ChangeTracker);
        var linhas = await ctx.SaveChangesAsync(ct);

        if (recursos.Length > 0)
        {
            try
            {
                // Só nomes de recursos, nunca dados: cada browser recarrega pela API,
                // que aplica as regras de visibilidade do perfil.
                await hub.Clients.All.SendAsync("alterado", recursos, CancellationToken.None);
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
