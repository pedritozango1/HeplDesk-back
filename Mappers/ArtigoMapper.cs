using Novati.API.Dtos.BaseConhecimento;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class ArtigoMapper
{
    // As contagens só são exatas se as Avaliacoes vierem carregadas (GetTodosComAvaliacoesAsync);
    // num artigo acabado de criar a lista está vazia, o que também é o valor certo.
    public static ArtigoDto ToDto(Artigo a) => new(a.Id, a.Titulo, a.Conteudo, a.Tags, a.Categoria, a.AutorId,
        a.Avaliacoes.Count(x => x.Util), a.Avaliacoes.Count(x => !x.Util));

    public static Artigo ToEntity(CreateArtigoRequest r, Guid autorId) => new()
    {
        Titulo = r.Titulo,
        Conteudo = r.Conteudo,
        Tags = r.Tags,
        Categoria = r.Categoria,
        AutorId = autorId,
    };
}
