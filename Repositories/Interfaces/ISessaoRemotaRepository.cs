using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Repositories.Interfaces;

public interface ISessaoRemotaRepository : IRepository<SessaoRemota>
{
    /// <summary>A sessão PEDIDA, AUTORIZADA ou ATIVA desta solicitação (no máximo uma), ou null.</summary>
    Task<SessaoRemota?> GetEmCursoPorSolicitacaoAsync(Guid solicitacaoId, CancellationToken ct = default);

    /// <summary>FUNCIONARIO: as das suas solicitações. TECNICO: as que pediu. GESTOR/ADMIN: todas.</summary>
    Task<List<SessaoRemota>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default);

    /// <summary>
    /// Sessões em curso que já passaram do prazo: pedidas antes de <paramref name="pedidasAntesDe"/>,
    /// autorizadas com token vencido em <paramref name="agora"/>, ou ativas desde antes de <paramref name="ativasAntesDe"/>.
    /// </summary>
    Task<List<SessaoRemota>> GetVencidasAsync(DateTime pedidasAntesDe, DateTime agora, DateTime ativasAntesDe, CancellationToken ct = default);
}
