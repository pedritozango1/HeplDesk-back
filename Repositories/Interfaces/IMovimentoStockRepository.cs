using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

public interface IMovimentoStockRepository : IRepository<MovimentoStock>
{
    /// <summary>Todos os movimentos, mais recentes primeiro.</summary>
    Task<List<MovimentoStock>> GetAllOrderedAsync(CancellationToken ct = default);

    /// <summary>
    /// Página de movimentos, mais recentes primeiro. Pesquisa em componente e observação;
    /// filtro opcional por tipo.
    /// </summary>
    Task<PaginaResultado<MovimentoStock>> GetPaginaAsync(PaginacaoQuery paginacao, TipoMovimento? tipo, CancellationToken ct = default);
}
