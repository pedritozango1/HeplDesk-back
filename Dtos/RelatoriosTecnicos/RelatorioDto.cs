namespace Novati.API.Dtos.RelatoriosTecnicos;

/// <summary>Peça registada no relatório técnico.</summary>
public record PecaRelatorioDto(string Nome, int Quantidade, string Codigo);

/// <summary>Linha de auditoria do relatório (uma por alteração de campo).</summary>
public record RelatorioHistoricoDto(DateOnly Data, Guid AutorId, string Acao, string Campo, string ValorAntigo, string ValorNovo);

/// <summary>Relatório técnico de uma ordem resolvida, com o histórico de auditoria.</summary>
public record RelatorioDto(
    Guid Id,
    Guid OrdemId,
    Guid AutorId,
    DateOnly CriadoEm,
    DateOnly AtualizadoEm,
    string Status,
    string Sumario,
    string Diagnostico,
    string SolucaoAplicada,
    List<PecaRelatorioDto> PecasUsadas,
    int? TempoGastoMin,
    string Procedimentos,
    string Observacoes,
    string AssinaturaTecnico,
    string AssinaturaResponsavel,
    string ComentariosInternos,
    Guid? AssinaturaTecnicoFicheiroId,
    List<RelatorioHistoricoDto> Historico
);
