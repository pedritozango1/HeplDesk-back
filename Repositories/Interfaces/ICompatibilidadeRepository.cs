using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface ICompatibilidadeRepository : IRepository<Compatibilidade>
{
    /// <summary>Verifica se já existe uma compatibilidade para este par de modelos.</summary>
    Task<bool> ExistsPairAsync(Guid modeloDispositivoId, Guid modeloComponenteId, CancellationToken ct = default);

    /// <summary>
    /// Verifica se o modelo de componente tem alguma compatibilidade registada.
    /// Usado pelo Módulo 11 (instalar peça) para só bloquear a instalação quando o
    /// catálogo realmente restringe o componente — se não houver nenhuma, não há o que validar.
    /// </summary>
    Task<bool> ExisteAlgumaAsync(Guid modeloComponenteId, CancellationToken ct = default);
}
