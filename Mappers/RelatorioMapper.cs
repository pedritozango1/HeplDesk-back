using Novati.API.Dtos.RelatoriosTecnicos;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class RelatorioMapper
{
    public static RelatorioDto ToDto(RelatorioTecnico r) => new(
        r.Id,
        r.OrdemId,
        r.AutorId,
        r.CriadoEm,
        r.AtualizadoEm,
        r.Status.ToString(),
        r.Sumario,
        r.Diagnostico,
        r.SolucaoAplicada,
        // Regra de ouro nº 5: coleções nunca null.
        r.PecasUsadas.Select(p => new PecaRelatorioDto(p.Nome, p.Quantidade, p.Codigo)).ToList(),
        r.TempoGastoMin,
        r.Procedimentos,
        r.Observacoes,
        r.AssinaturaTecnico,
        r.AssinaturaResponsavel,
        r.ComentariosInternos,
        r.AssinaturaTecnicoFicheiroId,
        r.Historico
            .OrderBy(h => h.Data).ThenBy(h => h.Posicao)
            .Select(h => new RelatorioHistoricoDto(h.Data, h.AutorId, h.Acao.ToString(), h.Campo, h.ValorAntigo, h.ValorNovo))
            .ToList());
}
