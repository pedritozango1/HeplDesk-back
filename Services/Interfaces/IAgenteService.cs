using Novati.API.Dtos.Agente;

namespace Novati.API.Services.Interfaces;

public interface IAgenteService
{
    /// <summary>Gera um link de download com validade. Com funcionarioId, notifica-o com o link.</summary>
    Task<LinkAgenteDto> CriarLinkAsync(Guid autorId, Guid? funcionarioId, CancellationToken ct = default);

    AgenteInfoDto Info(string? token);

    /// <summary>
    /// Valida o token e devolve o caminho do executável e a marca a acrescentar-lhe no fim
    /// (o endereço do servidor, que o agente lê do próprio ficheiro).
    /// </summary>
    (string Caminho, byte[] Marca) ParaDownload(string? token, string servidor);
}
