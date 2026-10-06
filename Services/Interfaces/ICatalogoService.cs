using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Catalogo;

namespace Novati.API.Services.Interfaces;

public interface ICatalogoService
{
    Task<List<ModeloDispositivoDto>> GetModelosDispositivoAsync(CancellationToken ct = default);
    Task<PaginaResultado<ModeloDispositivoDto>> GetModelosDispositivoPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default);
    Task<ModeloDispositivoDto> CreateModeloDispositivoAsync(CreateModeloDispositivoRequest request, CancellationToken ct = default);
    Task<ModeloDispositivoDto> UpdateModeloDispositivoAsync(Guid id, UpdateModeloDispositivoRequest request, CancellationToken ct = default);
    Task DeleteModeloDispositivoAsync(Guid id, CancellationToken ct = default);

    Task<List<ModeloComponenteDto>> GetModelosComponenteAsync(CancellationToken ct = default);
    Task<PaginaResultado<ModeloComponenteDto>> GetModelosComponentePaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default);
    Task<ModeloComponenteDto> CreateModeloComponenteAsync(CreateModeloComponenteRequest request, CancellationToken ct = default);
    Task<ModeloComponenteDto> UpdateModeloComponenteAsync(Guid id, UpdateModeloComponenteRequest request, CancellationToken ct = default);
    Task DeleteModeloComponenteAsync(Guid id, CancellationToken ct = default);

    Task<List<CompatibilidadeDto>> GetCompatibilidadesAsync(CancellationToken ct = default);
    Task<CompatibilidadeDto> CreateCompatibilidadeAsync(CreateCompatibilidadeRequest request, CancellationToken ct = default);
    Task DeleteCompatibilidadeAsync(Guid id, CancellationToken ct = default);
}
