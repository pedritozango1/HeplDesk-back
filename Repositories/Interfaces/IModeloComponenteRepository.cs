using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface IModeloComponenteRepository : IRepository<ModeloComponente>
{
    /// <summary>Página de modelos por nome. Pesquisa em nome e capacidade; filtro opcional por tipo.</summary>
    Task<PaginaResultado<ModeloComponente>> GetPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default);
}
