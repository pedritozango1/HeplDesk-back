using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

/// <summary>Repositório de RelatorioTecnico, com o histórico de auditoria e a ordem sempre incluídos.</summary>
public interface IRelatorioRepository : IRepository<RelatorioTecnico>
{
    /// <summary>
    /// Relatórios filtrados por estado e/ou pelas solicitações que o utilizador pode ver.
    /// Usado na visibilidade por perfil do serviço (ADMIN/TECNICO tudo, GESTOR finalizado+aprovado,
    /// FUNCIONARIO só aprovados das suas solicitações).
    /// </summary>
    Task<List<RelatorioTecnico>> GetVisiveisAsync(
        Models.Enums.StatusRelatorio[]? estados = null,
        IEnumerable<Guid>? solicitacaoIds = null,
        CancellationToken ct = default);

    /// <summary>Um relatório pelo id, já com o histórico e a ordem carregados.</summary>
    Task<RelatorioTecnico?> GetComHistoricoAsync(Guid id, CancellationToken ct = default);
}
