using Novati.API.Dtos.Solicitacoes;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;

namespace Novati.API.Mappers;

public static class SolicitacaoMapper
{
    public static SolicitacaoDto ToDto(Solicitacao s) => new(
        s.Id, s.Titulo, s.Descricao, s.Estado.ToString(),
        s.DispositivoFisicoId, s.SolicitanteId, s.DataCriacao,
        s.Prioridade.ToString(), s.Categoria,
        s.Anexos.Select(a => new AnexoDto(a.FicheiroId, a.Nome, a.Tipo, a.DataUrl)).ToList(),
        s.Avaliacao is null ? null : new AvaliacaoDto(s.Avaliacao.Estrelas, s.Avaliacao.Comentario, s.Avaliacao.Data),
        s.ResolvidaViaBase
    );

    /// <summary>Monta a entidade a partir do request. Prioridade e ficheiros dos anexos já vêm resolvidos pelo Service.</summary>
    public static Solicitacao ToEntity(CreateSolicitacaoRequest r, Guid solicitanteId, Prioridade prioridade, List<Ficheiro> anexos) => new()
    {
        Titulo = r.Titulo,
        Descricao = r.Descricao,
        Estado = EstadoSolicitacao.ABERTA,
        Prioridade = prioridade,
        Categoria = r.Categoria,
        DataCriacao = DateOnly.FromDateTime(DateTime.UtcNow),
        SolicitanteId = solicitanteId,
        DispositivoFisicoId = r.DispositivoFisicoId,
        Anexos = anexos.Select(f => new Anexo { FicheiroId = f.Id, Nome = f.NomeOriginal, Tipo = f.ContentType }).ToList(),
    };
}
