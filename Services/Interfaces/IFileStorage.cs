namespace Novati.API.Services.Interfaces;

/// <summary>
/// Onde ficam os bytes dos ficheiros. Hoje é o disco local (<c>LocalFileStorage</c>);
/// trocar para Azure Blob / S3 é só outra implementação desta interface.
/// </summary>
public interface IFileStorage
{
    /// <summary>Grava o conteúdo e devolve o caminho relativo gerado.</summary>
    Task<string> GuardarAsync(Stream conteudo, string extensao, CancellationToken ct = default);

    /// <summary>Abre o ficheiro para leitura. Lança NotFoundException se não existir.</summary>
    Stream Abrir(string caminhoRelativo);

    void Apagar(string caminhoRelativo);
}
