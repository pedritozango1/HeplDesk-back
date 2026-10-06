using Novati.API.Common.Paginacao;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

/// <summary>Repositório de RequisicaoCompra (Compras).</summary>
public interface ICompraRepository : IRepository<RequisicaoCompra>
{
    /// <summary>Todas as requisições, em ordem de criação crescente (o front inverte).</summary>
    Task<List<RequisicaoCompra>> GetAllOrderedAsync(CancellationToken ct = default);

    /// <summary>
    /// Página de requisições, mais recentes primeiro. Pesquisa em componente, justificativa e
    /// solicitante; filtro opcional por estado.
    /// </summary>
    Task<PaginaResultado<RequisicaoCompra>> GetPaginaAsync(PaginacaoQuery paginacao, EstadoRequisicao? estado, CancellationToken ct = default);
}
