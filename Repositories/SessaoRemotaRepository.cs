using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class SessaoRemotaRepository(AppDbContext ctx) : Repository<SessaoRemota>(ctx), ISessaoRemotaRepository
{
    public async Task<SessaoRemota?> GetEmCursoPorSolicitacaoAsync(Guid solicitacaoId, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(s => s.SolicitacaoId == solicitacaoId
            && (s.Estado == EstadoSessaoRemota.PEDIDA
                || s.Estado == EstadoSessaoRemota.AUTORIZADA
                || s.Estado == EstadoSessaoRemota.ATIVA), ct);

    public async Task<List<SessaoRemota>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking();

        if (role == Role.FUNCIONARIO)
            q = q.Where(s => s.SolicitanteId == userId);
        else if (role is not (Role.ADMIN or Role.GESTOR))
            q = q.Where(s => s.TecnicoId == userId);

        return await q.OrderByDescending(s => s.PedidaEm).ToListAsync(ct);
    }

    public async Task<List<SessaoRemota>> GetVencidasAsync(DateTime pedidasAntesDe, DateTime agora, DateTime ativasAntesDe, CancellationToken ct = default)
        => await Set.Where(s =>
                (s.Estado == EstadoSessaoRemota.PEDIDA && s.PedidaEm < pedidasAntesDe)
                || (s.Estado == EstadoSessaoRemota.AUTORIZADA && s.TokenExpiraEm < agora)
                || (s.Estado == EstadoSessaoRemota.ATIVA && s.IniciadaEm < ativasAntesDe))
            .ToListAsync(ct);
}
