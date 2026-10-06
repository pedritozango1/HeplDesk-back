namespace Novati.API.Repositories.Interfaces;

/// <summary>Ponto único de SaveChanges para garantir transações atómicas.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}