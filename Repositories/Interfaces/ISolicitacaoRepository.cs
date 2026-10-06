using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

public interface ISolicitacaoRepository : IRepository<Solicitacao>
{
    Task<List<Solicitacao>> GetBySolicitanteAsync(Guid solicitanteId, CancellationToken ct = default);

    /// <summary>
    /// Página de solicitações, mais recentes primeiro. Pesquisa em título e descrição; filtros
    /// opcionais por estado e por solicitante (null = de todos).
    /// </summary>
    Task<PaginaResultado<Solicitacao>> GetPaginaAsync(
        PaginacaoQuery paginacao, EstadoSolicitacao? estado, Guid? solicitanteId, CancellationToken ct = default);
}
