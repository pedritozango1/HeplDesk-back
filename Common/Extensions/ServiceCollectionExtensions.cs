using Novati.API.Data;
using Novati.API.Repositories;
using Novati.API.Repositories.Interfaces;
using Novati.API.Common.Settings;
using Novati.API.Services;
using Novati.API.Services.Assistente;
using Novati.API.Services.Interfaces;

namespace Novati.API.Common.Extensions;
/// <summary>Extensões para registar serviços no contentor de DI.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Regista repositórios, unit of work e services da aplicação.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // ─── Repositórios genéricos ────────────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ─── Módulo 6: Auth + Users ─────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        // ─── Módulo 7: Catálogo ─────────────────────────────
        services.AddScoped<IModeloDispositivoRepository, ModeloDispositivoRepository>();
        services.AddScoped<IModeloComponenteRepository, ModeloComponenteRepository>();
        services.AddScoped<ICompatibilidadeRepository, CompatibilidadeRepository>();
        services.AddScoped<ICatalogoService, CatalogoService>();

        // ─── Módulo 8: Localizações, Dispositivos, Instâncias ─
        services.AddScoped<ILocalizacaoRepository, LocalizacaoRepository>();
        services.AddScoped<IDispositivoRepository, DispositivoRepository>();
        services.AddScoped<IInstanciaRepository, InstanciaRepository>();
        services.AddScoped<IDispositivoService, DispositivoService>();

        // ─── Módulo 9: Stock ─────────────────────────────────
        services.AddScoped<IItemStockRepository, ItemStockRepository>();
        services.AddScoped<IUnidadeStockRepository, UnidadeStockRepository>();
        services.AddScoped<IMovimentoStockRepository, MovimentoStockRepository>();
        services.AddScoped<IStockService, StockService>();

        // ─── Módulo 10: Solicitações + Notificações + Base + Chat ──
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<ISolicitacaoRepository, SolicitacaoRepository>();
        services.AddScoped<ISolicitacaoService, SolicitacaoService>();
        services.AddScoped<IArtigoRepository, ArtigoRepository>();
        services.AddScoped<IBaseConhecimentoService, BaseConhecimentoService>();
        services.AddScoped<IMensagemRepository, MensagemRepository>();
        services.AddScoped<IOrdemRepository, OrdemRepository>();
        services.AddScoped<IChatService, ChatService>();

        // ─── Módulo 11: Atendimentos + Compras ───────────────
        services.AddScoped<ICompraRepository, CompraRepository>();
        services.AddScoped<IAtendimentoService, AtendimentoService>();
        services.AddScoped<IComprasService, ComprasService>();

        // ─── Módulo 12: Relatórios técnicos ─────────────────
        services.AddScoped<IRelatorioRepository, RelatorioRepository>();
        services.AddScoped<IRelatorioService, RelatorioService>();

        // ─── Módulo 14: Ficheiros (upload/download) ─────────
        services.Configure<StorageSettings>(config.GetSection("Storage"));
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IFicheiroRepository, FicheiroRepository>();
        services.AddScoped<IFicheiroService, FicheiroService>();

        // ─── Domínio (enums, categorias, SLA) ────────────────
        services.Configure<DominioSettings>(config.GetSection("Dominio"));
        services.AddSingleton<IDominioService, DominioService>();

        // ─── Assistente (motor de sugestões próprio, sem serviços externos) ─
        services.AddScoped<AssistenteService>();

        return services;
    }
}