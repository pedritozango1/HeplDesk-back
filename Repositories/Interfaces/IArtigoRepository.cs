using Novati.API.Models.Entities;

namespace Novati.API.Repositories.Interfaces;

public interface IArtigoRepository : IRepository<Artigo>
{
    /// <summary>Todos os artigos com as avaliações "Isto ajudou?" (para contagens e ordenação).</summary>
    Task<List<Artigo>> GetTodosComAvaliacoesAsync(CancellationToken ct = default);

    /// <summary>Avaliação deste utilizador para o artigo, ou null.</summary>
    Task<ArtigoAvaliacao?> GetAvaliacaoAsync(Guid artigoId, Guid userId, CancellationToken ct = default);

    Task AddAvaliacaoAsync(ArtigoAvaliacao avaliacao, CancellationToken ct = default);
}
