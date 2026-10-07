using Microsoft.EntityFrameworkCore;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Data;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>   options) : base(options){}
     public DbSet<User> Users => Set<User>();
    public DbSet<ModeloDispositivo> ModelosDispositivo => Set<ModeloDispositivo>();
    public DbSet<ModeloComponente> ModelosComponente => Set<ModeloComponente>();
    public DbSet<Compatibilidade> Compatibilidades => Set<Compatibilidade>();
    public DbSet<Localizacao> Localizacoes => Set<Localizacao>();
    public DbSet<DispositivoFisico> DispositivosFisicos => Set<DispositivoFisico>();
    public DbSet<InstanciaComponente> InstanciasComponentes => Set<InstanciaComponente>();
    public DbSet<ItemStock> ItensStock => Set<ItemStock>();
    public DbSet<UnidadeStock> UnidadesStock => Set<UnidadeStock>();
    public DbSet<MovimentoStock> MovimentosStock => Set<MovimentoStock>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<OrdemReparo> OrdensReparo => Set<OrdemReparo>();
    public DbSet<OrdemPecaUsada> OrdemPecasUsadas => Set<OrdemPecaUsada>();
    public DbSet<OrdemRejeicao> OrdemRejeicoes => Set<OrdemRejeicao>();
    public DbSet<OrdemHistorico> OrdemHistoricos => Set<OrdemHistorico>();
    public DbSet<RequisicaoCompra> RequisicoesCompra => Set<RequisicaoCompra>();
    public DbSet<Artigo> Artigos => Set<Artigo>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<Mensagem> Mensagens => Set<Mensagem>();
    public DbSet<RelatorioTecnico> RelatoriosTecnicos => Set<RelatorioTecnico>();
    public DbSet<RelatorioHistorico> RelatorioHistoricos => Set<RelatorioHistorico>();
    public DbSet<ArtigoAvaliacao> ArtigoAvaliacoes => Set<ArtigoAvaliacao>();
    public DbSet<Ficheiro> Ficheiros => Set<Ficheiro>();
    public DbSet<SessaoRemota> SessoesRemotas => Set<SessaoRemota>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> automaticamente
        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // O Id é gerado no C# (BaseEntity: Guid.NewGuid()), não pela BD. Sem isto, o EF
        // trata um filho novo com Id preenchido (ex.: ordem.Historico.Add) como já existente.
        foreach (var entity in mb.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            mb.Entity(entity.ClrType).Property(nameof(BaseEntity.Id)).ValueGeneratedNever();
        }
    }

     protected override void ConfigureConventions(ModelConfigurationBuilder cb)
    {
        // Todos os enums são guardados como TEXTO na BD (não como inteiro).
        // Vantagens: legível no pgAdmin; estável se reordenarmos os membros.
        cb.Properties<Role>().HaveConversion<string>();
        cb.Properties<Prioridade>().HaveConversion<string>();
        cb.Properties<EstadoSolicitacao>().HaveConversion<string>();
        cb.Properties<EstadoOrdem>().HaveConversion<string>();
        cb.Properties<EstadoDispositivo>().HaveConversion<string>();
        cb.Properties<EstadoInstancia>().HaveConversion<string>();
        cb.Properties<EstadoUnidade>().HaveConversion<string>();
        cb.Properties<TipoMovimento>().HaveConversion<string>();
        cb.Properties<EstadoRequisicao>().HaveConversion<string>();
        cb.Properties<TipoHistoricoOrdem>().HaveConversion<string>();
        cb.Properties<StatusRelatorio>().HaveConversion<string>();
        cb.Properties<AcaoRelatorio>().HaveConversion<string>();
        cb.Properties<EstadoSessaoRemota>().HaveConversion<string>();
        cb.Properties<ModoAcessoRemoto>().HaveConversion<string>();
    }
}
