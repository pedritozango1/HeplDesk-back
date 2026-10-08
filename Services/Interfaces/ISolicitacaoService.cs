using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Solicitacoes;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface ISolicitacaoService
{
    /// <summary>FUNCIONARIO só vê as suas; os restantes perfis veem todas.</summary>
    Task<List<SolicitacaoDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default);

    /// <summary>Versão paginada de <see cref="GetVisiveisAsync"/>, com a mesma regra de visibilidade.</summary>
    Task<PaginaResultado<SolicitacaoDto>> GetPaginaAsync(Guid userId, Role role, PaginacaoQuery paginacao, string? estado, CancellationToken ct = default);

    /// <summary>
    /// Um dispositivo atribuído a alguém só pode ser reportado pelo próprio, por um gestor ou por um admin;
    /// os dispositivos da sala (sem responsável) podem ser reportados por qualquer utilizador.
    /// </summary>
    Task<SolicitacaoDto> CreateAsync(Guid solicitanteId, Role role, CreateSolicitacaoRequest request, CancellationToken ct = default);
    Task<SolicitacaoDto> FecharAsync(Guid id, Guid userId, Role role, CancellationToken ct = default);
    Task<SolicitacaoDto> AvaliarAsync(Guid id, Guid userId, AvaliarSolicitacaoRequest request, CancellationToken ct = default);
    Task<SolicitacaoDto> ResolverViaBaseAsync(Guid id, Guid userId, CancellationToken ct = default);
}
