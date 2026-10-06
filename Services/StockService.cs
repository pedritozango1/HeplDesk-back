using Novati.API.Common.Exceptions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Stock;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Regras de negócio de stock: itens, unidades físicas e movimentos.</summary>
public class StockService(
    IItemStockRepository itens,
    IUnidadeStockRepository unidades,
    IMovimentoStockRepository movimentos,
    IModeloComponenteRepository modelosComponente,
    IUnitOfWork uow) : IStockService
{
    public async Task<List<ItemStockDto>> GetItensAsync(CancellationToken ct = default)
        => (await itens.GetAllAsync(ct)).Select(StockMapper.ToDto).ToList();

    public async Task<List<UnidadeDto>> GetUnidadesAsync(CancellationToken ct = default)
        => (await unidades.GetAllAsync(ct)).Select(StockMapper.ToDto).ToList();

    public async Task<List<MovimentoDto>> GetMovimentosAsync(CancellationToken ct = default)
        => (await movimentos.GetAllOrderedAsync(ct)).Select(StockMapper.ToDto).ToList();

    public async Task<PaginaResultado<UnidadeDto>> GetUnidadesPaginaAsync(PaginacaoQuery paginacao, string? estado, CancellationToken ct = default)
    {
        var filtroEstado = FiltroEnum.Ler<EstadoUnidade>(estado, "estado");
        return (await unidades.GetPaginaAsync(paginacao, filtroEstado, ct)).Map(StockMapper.ToDto);
    }

    public async Task<PaginaResultado<MovimentoDto>> GetMovimentosPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default)
    {
        var filtroTipo = FiltroEnum.Ler<TipoMovimento>(tipo, "tipo");
        return (await movimentos.GetPaginaAsync(paginacao, filtroTipo, ct)).Map(StockMapper.ToDto);
    }

    public async Task<List<UnidadeDto>> EntradaAsync(EntradaStockRequest request, CancellationToken ct = default)
    {
        var observacao = string.IsNullOrWhiteSpace(request.Observacao) ? "Entrada manual" : request.Observacao;

        var criadas = await CriarUnidadesAsync(request.ModeloComponenteId, request.Quantidade, observacao, TipoMovimento.ENTRADA, ct);

        return criadas.Select(StockMapper.ToDto).ToList();
    }

    public async Task<List<UnidadeStock>> CriarUnidadesAsync(Guid modeloComponenteId, int quantidade, string observacao, TipoMovimento tipo, CancellationToken ct = default)
    {
        if (quantidade < 1)
            throw new BusinessRuleException("Quantidade tem de ser pelo menos 1.");

        var modelo = await modelosComponente.GetByIdAsync(modeloComponenteId, ct)
                     ?? throw new NotFoundException("Modelo de componente não encontrado.");

        var item = await itens.GetOrCreateByModeloAsync(modeloComponenteId, ct);

        // Prefixo do código pelo tipo do modelo (ex.: "SSD" → "SSD", "Fonte" → "FNT").
        var prefixo = PrefixoParaTipo(modelo.Tipo);

        // Próximo número livre: 1 única query, depois cálculo em memória (evita N idas à BD).
        var codigosExistentes = await unidades.GetCodigosComPrefixoAsync(prefixo, ct);
        var proximoNumero = codigosExistentes
            .Select(c => int.TryParse(c.AsSpan(prefixo.Length + 1), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var novasUnidades = new List<UnidadeStock>();
        for (var i = 0; i < quantidade; i++)
        {
            var unidade = new UnidadeStock
            {
                ItemStockId = item.Id,
                Codigo = $"{prefixo}-{proximoNumero + i:000}",
                Estado = EstadoUnidade.DISPONIVEL,
            };
            novasUnidades.Add(unidade);
            await unidades.AddAsync(unidade, ct);
        }

        var movimento = new MovimentoStock
        {
            ItemStockId = item.Id,
            Tipo = tipo,
            Quantidade = quantidade,
            Data = DateOnly.FromDateTime(DateTime.UtcNow),
            Observacao = observacao,
        };
        await movimentos.AddAsync(movimento, ct);

        // Tudo (item novo + unidades + movimento) grava numa só transação.
        await uow.SaveChangesAsync(ct);

        return novasUnidades;
    }

    private static string PrefixoParaTipo(string tipo) => tipo switch
    {
        "SSD" => "SSD",
        "RAM" => "RAM",
        "Fonte" => "FNT",
        "Bateria" => "BAT",
        _ => tipo.Length >= 3 ? tipo[..3].ToUpperInvariant() : tipo.ToUpperInvariant(),
    };
}
