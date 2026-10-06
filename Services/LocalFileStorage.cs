using Microsoft.Extensions.Options;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Settings;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Armazenamento em disco: {RootPath}/aaaa/MM/{guid}{extensão}.</summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _raiz;

    public LocalFileStorage(IOptions<StorageSettings> options, IWebHostEnvironment env)
    {
        _raiz = Path.GetFullPath(Path.Combine(env.ContentRootPath, options.Value.RootPath));
        Directory.CreateDirectory(_raiz);
    }

    public async Task<string> GuardarAsync(Stream conteudo, string extensao, CancellationToken ct = default)
    {
        // O nome no disco é gerado pelo servidor — o nome do cliente nunca entra no caminho.
        var agora = DateTime.UtcNow;
        var relativo = Path.Combine(agora.ToString("yyyy"), agora.ToString("MM"), $"{Guid.NewGuid():N}{extensao}");
        var completo = CaminhoSeguro(relativo);

        Directory.CreateDirectory(Path.GetDirectoryName(completo)!);
        await using var destino = new FileStream(completo, FileMode.CreateNew, FileAccess.Write);
        await conteudo.CopyToAsync(destino, ct);

        return relativo.Replace(Path.DirectorySeparatorChar, '/');
    }

    public Stream Abrir(string caminhoRelativo)
    {
        var completo = CaminhoSeguro(caminhoRelativo);
        if (!File.Exists(completo))
            throw new NotFoundException("O conteúdo do ficheiro já não existe no armazenamento.");
        return new FileStream(completo, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
    }

    public void Apagar(string caminhoRelativo)
    {
        var completo = CaminhoSeguro(caminhoRelativo);
        if (File.Exists(completo))
            File.Delete(completo);
    }

    // Defesa contra path traversal ("../../appsettings.json"): o caminho final tem de ficar dentro da raiz.
    private string CaminhoSeguro(string relativo)
    {
        var completo = Path.GetFullPath(Path.Combine(_raiz, relativo));
        if (!completo.StartsWith(_raiz + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Caminho de ficheiro inválido.");
        return completo;
    }
}
