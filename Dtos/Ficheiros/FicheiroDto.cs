namespace Novati.API.Dtos.Ficheiros;

/// <summary>Metadados de um ficheiro carregado. O conteúdo está em GET /api/ficheiros/{id}/conteudo.</summary>
public record FicheiroDto(
    Guid Id,
    string Nome,
    string Tipo,
    long TamanhoBytes,
    DateTime CriadoEm,
    Guid? CriadoPorId
);
