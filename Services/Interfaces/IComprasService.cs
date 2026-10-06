using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Compras;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IComprasService
{
    /// <summary>ADMIN/GESTOR veem todas as requisições; os restantes perfis recebem uma lista vazia (nunca 403).</summary>
    Task<List<RequisicaoDto>> GetVisiveisAsync(Role role, CancellationToken ct = default);

    /// <summary>Versão paginada de <see cref="GetVisiveisAsync"/>, com a mesma regra de visibilidade.</summary>
    Task<PaginaResultado<RequisicaoDto>> GetPaginaAsync(Role role, PaginacaoQuery paginacao, string? estado, CancellationToken ct = default);

    Task<RequisicaoDto> AprovarAsync(Guid id, CancellationToken ct = default);
    Task<RequisicaoDto> DarEntradaAsync(Guid id, CancellationToken ct = default);
    Task<RequisicaoDto> RecusarAsync(Guid id, CancellationToken ct = default);
}
