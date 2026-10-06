using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Stock;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IStockService
{
    Task<List<ItemStockDto>> GetItensAsync(CancellationToken ct = default);
    Task<List<UnidadeDto>> GetUnidadesAsync(CancellationToken ct = default);
    Task<List<MovimentoDto>> GetMovimentosAsync(CancellationToken ct = default);
    Task<PaginaResultado<UnidadeDto>> GetUnidadesPaginaAsync(PaginacaoQuery paginacao, string? estado, CancellationToken ct = default);
    Task<PaginaResultado<MovimentoDto>> GetMovimentosPaginaAsync(PaginacaoQuery paginacao, string? tipo, CancellationToken ct = default);
    Task<List<UnidadeDto>> EntradaAsync(EntradaStockRequest request, CancellationToken ct = default);

    /// <summary>
    /// Cria <paramref name="quantidade"/> unidades DISPONIVEL para o modelo de componente + 1 movimento.
    /// Método público reutilizado pelo Módulo 11 (Compras) — não duplicar esta lógica lá.
    /// </summary>
    Task<List<UnidadeStock>> CriarUnidadesAsync(Guid modeloComponenteId, int quantidade, string observacao, TipoMovimento tipo, CancellationToken ct = default);
}
