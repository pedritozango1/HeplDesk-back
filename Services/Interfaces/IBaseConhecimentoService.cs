using Novati.API.Dtos.BaseConhecimento;

namespace Novati.API.Services.Interfaces;

public interface IBaseConhecimentoService
{
    Task<List<ArtigoDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Artigos mais relevantes para o texto (máx. 3, por pontuação decrescente).</summary>
    Task<List<ArtigoDto>> BuscarAsync(string? texto, CancellationToken ct = default);

    Task<ArtigoDto> CreateAsync(Guid autorId, CreateArtigoRequest request, CancellationToken ct = default);

    /// <summary>"Isto ajudou?" — cria ou substitui a avaliação do utilizador para o artigo.</summary>
    Task<ArtigoDto> AvaliarAsync(Guid userId, Guid artigoId, bool util, CancellationToken ct = default);
}
