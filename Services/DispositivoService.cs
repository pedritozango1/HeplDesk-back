using Novati.API.Common.Exceptions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Dispositivos;
using Novati.API.Mappers;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Regras de negócio do inventário: localizações, dispositivos físicos e instâncias de componente.</summary>
public class DispositivoService(
    ILocalizacaoRepository localizacoes,
    IDispositivoRepository dispositivos,
    IInstanciaRepository instancias,
    IModeloDispositivoRepository modelosDispositivo,
    IModeloComponenteRepository modelosComponente,
    ICompatibilidadeRepository compatibilidades,
    IUnitOfWork uow) : IDispositivoService
{
    // ─── Localizações ───────────────────────────────────
    public async Task<List<LocalizacaoDto>> GetLocalizacoesAsync(CancellationToken ct = default)
        => (await localizacoes.GetAllAsync(ct)).Select(DispositivoMapper.ToDto).ToList();

    public async Task<LocalizacaoDto> CreateLocalizacaoAsync(CreateLocalizacaoRequest request, CancellationToken ct = default)
    {
        if (request.Pai.HasValue && !await localizacoes.ExistsAsync(request.Pai.Value, ct))
            throw new NotFoundException("Localização-pai não encontrada.");

        var localizacao = DispositivoMapper.ToEntity(request);

        await localizacoes.AddAsync(localizacao, ct);
        await uow.SaveChangesAsync(ct);

        return DispositivoMapper.ToDto(localizacao);
    }

    // ─── Dispositivos físicos ───────────────────────────
    public async Task<List<DispositivoDto>> GetDispositivosAsync(CancellationToken ct = default)
        => (await dispositivos.GetAllAsync(ct)).Select(DispositivoMapper.ToDto).ToList();

    public async Task<PaginaResultado<DispositivoDto>> GetDispositivosPaginaAsync(PaginacaoQuery paginacao, string? estado, CancellationToken ct = default)
    {
        var filtroEstado = FiltroEnum.Ler<EstadoDispositivo>(estado, "estado");
        return (await dispositivos.GetPaginaAsync(paginacao, filtroEstado, ct)).Map(DispositivoMapper.ToDto);
    }

    public async Task<DispositivoDto> CreateDispositivoAsync(CreateDispositivoRequest request, CancellationToken ct = default)
    {
        if (await dispositivos.PatrimonioExistsAsync(request.Patrimonio, ct))
            throw new ConflictException("Já existe um dispositivo com este nº de património.");

        if (!await modelosDispositivo.ExistsAsync(request.ModeloDispositivoId, ct))
            throw new NotFoundException("Modelo de dispositivo não encontrado.");

        if (!await localizacoes.ExistsAsync(request.LocalizacaoId, ct))
            throw new NotFoundException("Localização não encontrada.");

        var dispositivo = DispositivoMapper.ToEntity(request);

        await dispositivos.AddAsync(dispositivo, ct);
        await uow.SaveChangesAsync(ct);

        return DispositivoMapper.ToDto(dispositivo);
    }

    public async Task<DispositivoDto> UpdateEstadoAsync(Guid id, UpdateEstadoDispositivoRequest request, CancellationToken ct = default)
    {
        var dispositivo = await dispositivos.GetByIdAsync(id, ct)
                           ?? throw new NotFoundException("Dispositivo não encontrado.");

        if (!Enum.TryParse<EstadoDispositivo>(request.Estado, out var estado))
            throw new BusinessRuleException("Estado inválido.");

        dispositivo.Estado = estado;

        dispositivos.Update(dispositivo);
        await uow.SaveChangesAsync(ct);

        return DispositivoMapper.ToDto(dispositivo);
    }

    // ─── Instâncias de componente ───────────────────────
    public async Task<List<InstanciaDto>> GetInstanciasAsync(CancellationToken ct = default)
        => (await instancias.GetAllAsync(ct)).Select(DispositivoMapper.ToDto).ToList();

    public async Task<InstanciaDto> CreateInstanciaAsync(Guid dispositivoId, CreateInstanciaRequest request, CancellationToken ct = default)
    {
        var dispositivo = await dispositivos.GetByIdAsync(dispositivoId, ct)
                           ?? throw new NotFoundException("Dispositivo não encontrado.");

        if (!await modelosComponente.ExistsAsync(request.ModeloComponenteId, ct))
            throw new NotFoundException("Modelo de componente não encontrado.");

        // Só se pode instalar um componente compatível com o modelo do dispositivo (Módulo 7).
        if (!await compatibilidades.ExistsPairAsync(dispositivo.ModeloDispositivoId, request.ModeloComponenteId, ct))
            throw new BusinessRuleException("Componente incompatível com este modelo.");

        var instancia = DispositivoMapper.ToEntity(dispositivoId, request);

        await instancias.AddAsync(instancia, ct);
        await uow.SaveChangesAsync(ct);

        return DispositivoMapper.ToDto(instancia);
    }
}
