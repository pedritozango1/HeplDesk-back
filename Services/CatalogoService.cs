using Microsoft.EntityFrameworkCore;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Catalogo;
using Novati.API.Mappers;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Regras de negócio do catálogo (modelos de dispositivo/componente e compatibilidades).</summary>
public class CatalogoService(
    IModeloDispositivoRepository modelosDispositivo,
    IModeloComponenteRepository modelosComponente,
    ICompatibilidadeRepository compatibilidades,
    IUnitOfWork uow) : ICatalogoService
{
    // ─── ModeloDispositivo ──────────────────────────────
    public async Task<List<ModeloDispositivoDto>> GetModelosDispositivoAsync(CancellationToken ct = default)
        => (await modelosDispositivo.GetAllAsync(ct)).Select(CatalogoMapper.ToDto).ToList();

    public async Task<PaginaResultado<ModeloDispositivoDto>> GetModelosDispositivoPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default)
        => (await modelosDispositivo.GetPaginaAsync(paginacao, tipo, ct)).Map(CatalogoMapper.ToDto);

    public async Task<ModeloDispositivoDto> CreateModeloDispositivoAsync(CreateModeloDispositivoRequest request, CancellationToken ct = default)
    {
        var modelo = CatalogoMapper.ToEntity(request);

        await modelosDispositivo.AddAsync(modelo, ct);
        await uow.SaveChangesAsync(ct);

        return CatalogoMapper.ToDto(modelo);
    }

    public async Task<ModeloDispositivoDto> UpdateModeloDispositivoAsync(Guid id, UpdateModeloDispositivoRequest request, CancellationToken ct = default)
    {
        var modelo = await modelosDispositivo.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Modelo de dispositivo não encontrado.");

        CatalogoMapper.ApplyUpdate(modelo, request);

        modelosDispositivo.Update(modelo);
        await uow.SaveChangesAsync(ct);

        return CatalogoMapper.ToDto(modelo);
    }

    public async Task DeleteModeloDispositivoAsync(Guid id, CancellationToken ct = default)
    {
        var modelo = await modelosDispositivo.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Modelo de dispositivo não encontrado.");

        try
        {
            modelosDispositivo.Remove(modelo);
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Existem dispositivos deste modelo.");
        }
    }

    // ─── ModeloComponente ───────────────────────────────
    public async Task<List<ModeloComponenteDto>> GetModelosComponenteAsync(CancellationToken ct = default)
        => (await modelosComponente.GetAllAsync(ct)).Select(CatalogoMapper.ToDto).ToList();

    public async Task<PaginaResultado<ModeloComponenteDto>> GetModelosComponentePaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default)
        => (await modelosComponente.GetPaginaAsync(paginacao, tipo, ct)).Map(CatalogoMapper.ToDto);

    public async Task<ModeloComponenteDto> CreateModeloComponenteAsync(CreateModeloComponenteRequest request, CancellationToken ct = default)
    {
        var modelo = CatalogoMapper.ToEntity(request);
        if (request.StockMinimo.HasValue)
            modelo.StockMinimo = request.StockMinimo.Value;

        await modelosComponente.AddAsync(modelo, ct);
        await uow.SaveChangesAsync(ct);

        return CatalogoMapper.ToDto(modelo);
    }

    public async Task<ModeloComponenteDto> UpdateModeloComponenteAsync(Guid id, UpdateModeloComponenteRequest request, CancellationToken ct = default)
    {
        var modelo = await modelosComponente.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Modelo de componente não encontrado.");

        CatalogoMapper.ApplyUpdate(modelo, request);
        if (request.StockMinimo.HasValue)
            modelo.StockMinimo = request.StockMinimo.Value;

        modelosComponente.Update(modelo);
        await uow.SaveChangesAsync(ct);

        return CatalogoMapper.ToDto(modelo);
    }

    public async Task DeleteModeloComponenteAsync(Guid id, CancellationToken ct = default)
    {
        var modelo = await modelosComponente.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Modelo de componente não encontrado.");

        try
        {
            modelosComponente.Remove(modelo);
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Existem instâncias ou stock associados a este modelo.");
        }
    }

    // ─── Compatibilidade ────────────────────────────────
    public async Task<List<CompatibilidadeDto>> GetCompatibilidadesAsync(CancellationToken ct = default)
        => (await compatibilidades.GetAllAsync(ct)).Select(CatalogoMapper.ToDto).ToList();

    public async Task<CompatibilidadeDto> CreateCompatibilidadeAsync(CreateCompatibilidadeRequest request, CancellationToken ct = default)
    {
        if (!await modelosDispositivo.ExistsAsync(request.ModeloDispositivoId, ct))
            throw new NotFoundException("Modelo de dispositivo não encontrado.");

        if (!await modelosComponente.ExistsAsync(request.ModeloComponenteId, ct))
            throw new NotFoundException("Modelo de componente não encontrado.");

        if (await compatibilidades.ExistsPairAsync(request.ModeloDispositivoId, request.ModeloComponenteId, ct))
            throw new ConflictException("Esta compatibilidade já existe.");

        var compat = CatalogoMapper.ToEntity(request);

        await compatibilidades.AddAsync(compat, ct);
        await uow.SaveChangesAsync(ct);

        return CatalogoMapper.ToDto(compat);
    }

    public async Task DeleteCompatibilidadeAsync(Guid id, CancellationToken ct = default)
    {
        var compat = await compatibilidades.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Compatibilidade não encontrada.");

        compatibilidades.Remove(compat);
        await uow.SaveChangesAsync(ct);
    }
}
