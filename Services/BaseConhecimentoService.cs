using Novati.API.Common.Exceptions;
using Novati.API.Dtos.BaseConhecimento;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Base de conhecimento: artigos de autoajuda e pesquisa por pontuação de termos (RelevanciaTexto).</summary>
public class BaseConhecimentoService(
    IArtigoRepository artigos,
    IUnitOfWork uow) : IBaseConhecimentoService
{
    private const int LimiteResultados = 3;

    public async Task<List<ArtigoDto>> GetAllAsync(CancellationToken ct = default)
        => (await artigos.GetTodosComAvaliacoesAsync(ct)).Select(ArtigoMapper.ToDto).ToList();

    public async Task<List<ArtigoDto>> BuscarAsync(string? texto, CancellationToken ct = default)
    {
        var termos = RelevanciaTexto.Tokenizar(texto);
        if (termos.Count == 0)
            return [];

        var todos = await artigos.GetTodosComAvaliacoesAsync(ct);

        var pontuados = todos
            .Select(a => (Artigo: a, Pontos: RelevanciaTexto.PontuarArtigo(a, termos)))
            .Where(x => x.Pontos > 0)
            .OrderByDescending(x => x.Pontos)
            .Take(LimiteResultados)
            .Select(x => ArtigoMapper.ToDto(x.Artigo))
            .ToList();

        return pontuados;
    }

    public async Task<ArtigoDto> CreateAsync(Guid autorId, CreateArtigoRequest request, CancellationToken ct = default)
    {
        var artigo = ArtigoMapper.ToEntity(request, autorId);
        await artigos.AddAsync(artigo, ct);
        await uow.SaveChangesAsync(ct);

        return ArtigoMapper.ToDto(artigo);
    }

    public async Task<ArtigoDto> AvaliarAsync(Guid userId, Guid artigoId, bool util, CancellationToken ct = default)
    {
        if (!await artigos.ExistsAsync(artigoId, ct))
            throw new NotFoundException("Artigo não encontrado.");

        var existente = await artigos.GetAvaliacaoAsync(artigoId, userId, ct);
        if (existente is null)
        {
            await artigos.AddAvaliacaoAsync(new ArtigoAvaliacao
            {
                ArtigoId = artigoId,
                UserId = userId,
                Util = util,
                Data = DateOnly.FromDateTime(DateTime.Now),
            }, ct);
        }
        else
        {
            // Votar outra vez substitui a avaliação anterior (uma por pessoa).
            existente.Util = util;
            existente.Data = DateOnly.FromDateTime(DateTime.Now);
        }
        await uow.SaveChangesAsync(ct);

        var atualizado = (await artigos.GetTodosComAvaliacoesAsync(ct)).First(a => a.Id == artigoId);
        return ArtigoMapper.ToDto(atualizado);
    }
}
