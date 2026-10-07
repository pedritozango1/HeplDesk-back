using Novati.API.Services.Interfaces;

namespace Novati.API.BackgroundServices;

/// <summary>
/// De 30 em 30 segundos fecha as sessões remotas que passaram do prazo: pedidos sem
/// resposta, códigos por usar e sessões ativas há demasiado tempo. As regras de prazo
/// também são verificadas em cada operação — este serviço só garante que uma sessão
/// esquecida não fica aberta à espera de que alguém lhe toque.
/// </summary>
public class ExpiracaoSessoesRemotasService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiracaoSessoesRemotasService> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    // Este serviço vive o tempo todo da aplicação (singleton); o DbContext vive um
                    // pedido (scoped). Cria-se um scope por volta para ter um DbContext novo.
                    using var scope = scopeFactory.CreateScope();
                    var acessoRemoto = scope.ServiceProvider.GetRequiredService<IAcessoRemotoService>();

                    var fechadas = await acessoRemoto.ExpirarVencidasAsync(stoppingToken);
                    if (fechadas > 0)
                        logger.LogInformation("Sessões remotas fechadas por prazo: {Fechadas}", fechadas);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Uma volta falhada (BD em baixo, conflito de gravação) não pode matar o serviço.
                    logger.LogWarning(ex, "Falha ao expirar sessões remotas");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Aplicação a encerrar.
        }
    }
}
