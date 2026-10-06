namespace Novati.API.Dtos.Atendimentos;

/// <summary>Peça reservada/instalada por uma ordem de reparo.</summary>
public record PecaUsadaDto(Guid UnidadeStockId, Guid ModeloComponenteId, Guid? InstaladoInstanciaId);

/// <summary>Motivo de recusa da solução validada pelo funcionário.</summary>
public record RejeicaoDto(string Motivo, DateOnly Data);

/// <summary>Linha do histórico da ordem. <c>Autor</c> é o nome de quem escreveu.</summary>
public record HistoricoDto(Guid Id, string Tipo, string Texto, string Autor, DateOnly Data);

/// <summary>
/// Ordem de reparo, com as coleções aninhadas que o front assume sempre preenchidas.
/// Os tempos são cronometrados pelo servidor: <c>TempoGastoMin</c> = assumir → aprovação;
/// <c>TempoTrabalhoMin</c> = o mesmo sem as esperas pela validação. Ambos null até à aprovação.
/// </summary>
public record OrdemDto(
    Guid Id,
    Guid SolicitacaoId,
    Guid TecnicoId,
    string Diagnostico,
    string? Solucao,
    string? SolucaoSugerida,
    string Estado,
    List<PecaUsadaDto> PecasUsadas,
    int? TempoGastoMin,
    DateOnly DataInicio,
    DateOnly? DataFim,
    List<RejeicaoDto> Rejeicoes,
    List<HistoricoDto> Historico,
    DateTime IniciadaEm,
    DateTime? ConcluidaEm,
    int? TempoTrabalhoMin
);
