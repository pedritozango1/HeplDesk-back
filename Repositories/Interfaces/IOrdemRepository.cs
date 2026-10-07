using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

/// <summary>
/// Repositório de OrdemReparo. Começou no Módulo 10 (leitura, pelo Chat) e é
/// estendido no Módulo 11 (Atendimentos) com o CRUD completo.
/// </summary>
public interface IOrdemRepository : IRepository<OrdemReparo>
{
    /// <summary>Ids das solicitações para as quais este técnico tem uma ordem atribuída.</summary>
    Task<List<Guid>> GetSolicitacaoIdsPorTecnicoAsync(Guid tecnicoId, CancellationToken ct = default);

    /// <summary>Id do técnico com ordem sobre esta solicitação, ou null se não houver.</summary>
    Task<Guid?> GetTecnicoIdPorSolicitacaoAsync(Guid solicitacaoId, CancellationToken ct = default);

    /// <summary>Carrega a ordem com PecasUsadas, Rejeicoes, Historico (e o respetivo Autor) e Solicitacao.</summary>
    Task<OrdemReparo?> GetCompletaAsync(Guid id, CancellationToken ct = default);

    /// <summary>A ordem desta solicitação com o Historico carregado (para lhe acrescentar entradas), ou null.</summary>
    Task<OrdemReparo?> GetComHistoricoPorSolicitacaoAsync(Guid solicitacaoId, CancellationToken ct = default);

    /// <summary>Todas as ordens completas. Quem só pode ver as suas filtra por solicitacaoIds.</summary>
    Task<List<OrdemReparo>> GetTodasCompletasAsync(IEnumerable<Guid>? solicitacaoIds = null, CancellationToken ct = default);

    /// <summary>Id do relatório técnico já existente para esta ordem, ou null se ainda não houver.</summary>
    Task<Guid?> GetRelatorioIdAsync(Guid ordemId, CancellationToken ct = default);

    // Coleções filhas da ordem: pertencem ao mesmo agregado, por isso são geridas aqui.
    Task AddPecaAsync(OrdemPecaUsada peca, CancellationToken ct = default);
    Task AddRejeicaoAsync(OrdemRejeicao rejeicao, CancellationToken ct = default);
}
