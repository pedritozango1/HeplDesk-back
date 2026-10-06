using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface IItemStockRepository : IRepository<ItemStock>
{
    /// <summary>Devolve o ItemStock do modelo, criando-o (sem gravar) se ainda não existir.</summary>
    Task<ItemStock> GetOrCreateByModeloAsync(Guid modeloComponenteId, CancellationToken ct = default);
}
