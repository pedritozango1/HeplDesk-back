using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Novati.API.Models.Entities;

namespace Novati.API.Realtime;

/// <summary>
/// Traduz as entidades alteradas numa gravação para os nomes dos recursos do front
/// (as chaves de RECURSOS em novati/src/context/AppContext.jsx).
/// </summary>
public static class RecursosAlterados
{
    private static readonly Dictionary<Type, string> Mapa = new()
    {
        [typeof(User)] = "users",
        [typeof(ModeloDispositivo)] = "modelosDispositivo",
        [typeof(ModeloComponente)] = "modelosComponente",
        [typeof(Compatibilidade)] = "compatibilidades",
        [typeof(Localizacao)] = "localizacoes",
        [typeof(DispositivoFisico)] = "dispositivosFisicos",
        [typeof(InstanciaComponente)] = "instanciasComponentes",
        [typeof(ItemStock)] = "itensStock",
        [typeof(UnidadeStock)] = "unidadesStock",
        [typeof(MovimentoStock)] = "movimentosStock",
        [typeof(Solicitacao)] = "solicitacoes",
        [typeof(Anexo)] = "solicitacoes",
        [typeof(Avaliacao)] = "solicitacoes",
        [typeof(OrdemReparo)] = "ordensReparo",
        [typeof(OrdemHistorico)] = "ordensReparo",
        [typeof(OrdemRejeicao)] = "ordensReparo",
        [typeof(OrdemPecaUsada)] = "ordensReparo",
        [typeof(RequisicaoCompra)] = "requisicoesCompra",
        [typeof(Artigo)] = "artigos",
        [typeof(ArtigoAvaliacao)] = "artigos",
        [typeof(Notificacao)] = "notificacoes",
        [typeof(Mensagem)] = "conversas",
        [typeof(RelatorioTecnico)] = "relatoriosTecnicos",
        [typeof(RelatorioHistorico)] = "relatoriosTecnicos",
        [typeof(PecaRelatorio)] = "relatoriosTecnicos",
    };

    /// <summary>Recursos afetados pelas entradas pendentes do change tracker (antes do SaveChanges).</summary>
    public static string[] Pendentes(ChangeTracker tracker)
        => tracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => Mapa.GetValueOrDefault(e.Metadata.ClrType))
            .OfType<string>()
            .Distinct()
            .ToArray();
}
