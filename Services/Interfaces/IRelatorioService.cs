using Novati.API.Dtos.RelatoriosTecnicos;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IRelatorioService
{
    /// <summary>Lista filtrada por perfil: ADMIN/TECNICO veem tudo, GESTOR só finalizado+aprovado, FUNCIONARIO só aprovados das suas.</summary>
    Task<List<RelatorioDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default);

    Task<RelatorioDto> GetByIdAsync(Guid userId, Role role, Guid id, CancellationToken ct = default);

    Task<RelatorioDto> CreateAsync(Guid userId, Role role, CreateRelatorioRequest request, CancellationToken ct = default);

    /// <summary>Atualiza e regista uma linha de histórico por cada campo alterado.</summary>
    Task<RelatorioDto> UpdateAsync(Guid userId, Role role, Guid id, UpdateRelatorioRequest request, CancellationToken ct = default);

    Task<RelatorioDto> FinalizarAsync(Guid userId, Role role, Guid id, CancellationToken ct = default);
    Task<RelatorioDto> ReabrirAsync(Guid userId, Role role, Guid id, CancellationToken ct = default);
    Task<RelatorioDto> AprovarAsync(Guid userId, Role role, Guid id, CancellationToken ct = default);
}
