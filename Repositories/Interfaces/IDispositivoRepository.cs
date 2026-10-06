using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

public interface IDispositivoRepository : IRepository<DispositivoFisico>
{
    /// <summary>Verifica se já existe um dispositivo com este património.</summary>
    Task<bool> PatrimonioExistsAsync(string patrimonio, CancellationToken ct = default);

    /// <summary>
    /// Página de dispositivos por património. Pesquisa em património, modelo, localização e
    /// responsável; filtro opcional por estado.
    /// </summary>
    Task<PaginaResultado<DispositivoFisico>> GetPaginaAsync(PaginacaoQuery paginacao, EstadoDispositivo? estado, CancellationToken ct = default);
}
