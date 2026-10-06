using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface INotificacaoRepository : IRepository<Notificacao>
{
    /// <summary>Notificações de um utilizador, ordem cronológica crescente.</summary>
    Task<List<Notificacao>> GetByUserOrderedAsync(Guid userId, CancellationToken ct = default);
}
