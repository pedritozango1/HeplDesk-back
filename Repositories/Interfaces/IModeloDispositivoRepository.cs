using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface IModeloDispositivoRepository : IRepository<ModeloDispositivo>
{
    /// <summary>Página de modelos por nome. Pesquisa em nome e fabricante; filtro opcional por tipo.</summary>
    Task<PaginaResultado<ModeloDispositivo>> GetPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default);
}
