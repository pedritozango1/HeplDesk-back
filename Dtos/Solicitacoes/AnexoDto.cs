namespace Novati.API.Dtos.Solicitacoes;

/// <summary>Anexo de uma solicitação. Conteúdo em GET /api/ficheiros/{ficheiroId}/conteudo (ou dataUrl nos antigos).</summary>
public record AnexoDto(Guid? FicheiroId, string Nome, string Tipo, string? DataUrl);

/// <summary>Anexo a associar a uma solicitação nova: um ficheiro já carregado em POST /api/ficheiros.</summary>
public record AnexoRequest(Guid FicheiroId);
