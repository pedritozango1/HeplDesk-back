using Novati.API.Dtos.Atendimentos;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class OrdemMapper
{
    public static OrdemDto ToDto(OrdemReparo o) => new(
        o.Id,
        o.SolicitacaoId,
        o.TecnicoId,
        o.Diagnostico ?? "",
        o.Solucao,
        o.SolucaoSugerida,
        o.Estado.ToString(),
        // Regra de ouro nº 5: coleções nunca null — o front faz .map()/.length diretamente.
        o.PecasUsadas.Select(p => new PecaUsadaDto(p.UnidadeStockId, p.ModeloComponenteId, p.InstaladoInstanciaId)).ToList(),
        o.TempoGastoMin,
        o.DataInicio,
        o.DataFim,
        o.Rejeicoes.OrderBy(r => r.Data).Select(r => new RejeicaoDto(r.Motivo, r.Data)).ToList(),
        // O histórico expõe o NOME do autor, não o id.
        // Ordena pela entidade (Data + Posicao) antes de mapear: o DTO não expõe a Posicao.
        o.Historico.OrderBy(h => h.Data).ThenBy(h => h.Posicao)
                   .Select(h => new HistoricoDto(h.Id, h.Tipo.ToString(), h.Texto, h.Autor?.Nome ?? "", h.Data))
                   .ToList(),
        o.IniciadaEm,
        o.ConcluidaEm,
        o.TempoTrabalhoMin);
}
