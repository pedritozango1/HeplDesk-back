namespace Novati.API.Dtos.Solicitacoes;

/// <summary>Pedido de suporte aberto por um funcionário.</summary>
public record SolicitacaoDto(
    Guid Id,
    string Titulo,
    string Descricao,
    string Estado,
    Guid? DispositivoFisicoId,
    Guid SolicitanteId,
    DateOnly DataCriacao,
    string Prioridade,
    string Categoria,
    List<AnexoDto> Anexos,
    AvaliacaoDto? Avaliacao,
    bool ResolvidaViaBase,
    /// <summary>Instante exato da criação (UTC) — ordena pedidos do mesmo dia.</summary>
    DateTime CriadoEm
);
