using Microsoft.EntityFrameworkCore;
using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class MensagemRepository(AppDbContext ctx) : Repository<Mensagem>(ctx), IMensagemRepository
{
    public async Task<List<Mensagem>> GetRecentesAsync(Guid solicitacaoId, int saltar, int quantas, CancellationToken ct = default)
        => await Set.Where(m => m.SolicitacaoId == solicitacaoId)
                     // O Id desempata mensagens do mesmo instante: sem ele a ordem podia mudar entre pedidos.
                     .OrderByDescending(m => m.Data).ThenByDescending(m => m.Hora).ThenByDescending(m => m.Id)
                     .Skip(saltar).Take(quantas)
                     .ToListAsync(ct);

    public async Task<Dictionary<Guid, int>> ContarNaoLidasAsync(Guid userId, IReadOnlyCollection<Guid>? solicitacaoIds, CancellationToken ct = default)
        => await DasConversas(solicitacaoIds)
                     .Where(m => !m.Lida && m.AutorId != userId)
                     .GroupBy(m => m.SolicitacaoId)
                     .Select(g => new { SolicitacaoId = g.Key, Total = g.Count() })
                     .ToDictionaryAsync(x => x.SolicitacaoId, x => x.Total, ct);

    public async Task<List<Mensagem>> GetUltimasAsync(IReadOnlyCollection<Guid>? solicitacaoIds, CancellationToken ct = default)
    {
        // "Última" = não existe outra mais recente na mesma conversa (NOT EXISTS na BD).
        var candidatas = await DasConversas(solicitacaoIds)
            .Where(m => !Set.Any(o => o.SolicitacaoId == m.SolicitacaoId
                                      && (o.Data > m.Data || (o.Data == m.Data && o.Hora > m.Hora))))
            .ToListAsync(ct);

        // Duas mensagens no mesmo instante passam ambas no filtro: fica a de maior Id,
        // o mesmo desempate de GetRecentesAsync.
        return candidatas.GroupBy(m => m.SolicitacaoId)
                         .Select(g => g.MaxBy(m => m.Id)!)
                         .ToList();
    }

    private IQueryable<Mensagem> DasConversas(IReadOnlyCollection<Guid>? solicitacaoIds)
        => solicitacaoIds is null ? Set : Set.Where(m => solicitacaoIds.Contains(m.SolicitacaoId));

    public async Task<List<Mensagem>> GetPorLerAsync(Guid solicitacaoId, Guid userId, CancellationToken ct = default)
        => await Set.Where(m => m.SolicitacaoId == solicitacaoId && !m.Lida && m.AutorId != userId).ToListAsync(ct);
}
