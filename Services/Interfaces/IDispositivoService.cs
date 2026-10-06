using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Dispositivos;

namespace Novati.API.Services.Interfaces;

public interface IDispositivoService
{
    Task<List<LocalizacaoDto>> GetLocalizacoesAsync(CancellationToken ct = default);
    Task<LocalizacaoDto> CreateLocalizacaoAsync(CreateLocalizacaoRequest request, CancellationToken ct = default);

    Task<List<DispositivoDto>> GetDispositivosAsync(CancellationToken ct = default);
    Task<PaginaResultado<DispositivoDto>> GetDispositivosPaginaAsync(PaginacaoQuery paginacao, string? estado, CancellationToken ct = default);
    Task<DispositivoDto> CreateDispositivoAsync(CreateDispositivoRequest request, CancellationToken ct = default);
    Task<DispositivoDto> UpdateEstadoAsync(Guid id, UpdateEstadoDispositivoRequest request, CancellationToken ct = default);

    Task<List<InstanciaDto>> GetInstanciasAsync(CancellationToken ct = default);
    Task<InstanciaDto> CreateInstanciaAsync(Guid dispositivoId, CreateInstanciaRequest request, CancellationToken ct = default);
}
