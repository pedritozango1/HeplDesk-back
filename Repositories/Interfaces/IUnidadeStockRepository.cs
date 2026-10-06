using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

public interface IUnidadeStockRepository : IRepository<UnidadeStock>
{
    Task<bool> ExistsCodigoAsync(string codigo, CancellationToken ct = default);

    /// <summary>Devolve uma unidade DISPONIVEL deste item, ou null se não houver.</summary>
    Task<UnidadeStock?> GetDisponivelAsync(Guid itemStockId, CancellationToken ct = default);

    /// <summary>Todos os códigos existentes que começam por "{prefixo}-", para calcular o próximo número livre.</summary>
    Task<List<string>> GetCodigosComPrefixoAsync(string prefixo, CancellationToken ct = default);

    /// <summary>Página de unidades por código. Pesquisa em código e componente; filtro opcional por estado.</summary>
    Task<PaginaResultado<UnidadeStock>> GetPaginaAsync(PaginacaoQuery paginacao, EstadoUnidade? estado, CancellationToken ct = default);
}
