using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface IMensagemRepository : IRepository<Mensagem>
{
    /// <summary>
    /// Um bloco de mensagens de uma conversa, da mais recente para a mais antiga,
    /// saltando as <paramref name="saltar"/> mais recentes.
    /// </summary>
    Task<List<Mensagem>> GetRecentesAsync(Guid solicitacaoId, int saltar, int quantas, CancellationToken ct = default);

    /// <summary>
    /// Mensagens por ler (escritas por outros) por conversa. <paramref name="solicitacaoIds"/> null = todas as conversas.
    /// </summary>
    Task<Dictionary<Guid, int>> ContarNaoLidasAsync(Guid userId, IReadOnlyCollection<Guid>? solicitacaoIds, CancellationToken ct = default);

    /// <summary>
    /// A última mensagem de cada conversa. <paramref name="solicitacaoIds"/> null = todas as conversas.
    /// </summary>
    Task<List<Mensagem>> GetUltimasAsync(IReadOnlyCollection<Guid>? solicitacaoIds, CancellationToken ct = default);

    /// <summary>Mensagens de uma conversa ainda por ler, escritas por outros (para marcar como lidas).</summary>
    Task<List<Mensagem>> GetPorLerAsync(Guid solicitacaoId, Guid userId, CancellationToken ct = default);
}
