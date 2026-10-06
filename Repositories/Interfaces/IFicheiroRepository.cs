using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface IFicheiroRepository : IRepository<Ficheiro>
{
    Task<List<Ficheiro>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>True se o ficheiro for a assinatura de alguém, de um relatório ou anexo de uma solicitação.</summary>
    Task<bool> EstaEmUsoAsync(Guid id, CancellationToken ct = default);
}
