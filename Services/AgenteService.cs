using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Settings;
using Novati.API.Dtos.Agente;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>
/// Distribuição do Agente Novati: quem é da equipa (técnico, gestor, admin) gera um link com
/// validade; quem tem o link descarrega o executável já apontado a este servidor.
/// </summary>
public class AgenteService(
    IOptions<AgenteSettings> opcoes,
    IOptions<JwtSettings> jwt,
    IWebHostEnvironment ambiente,
    IUserRepository users,
    INotificacaoService notificacaoService,
    IUnitOfWork uow) : IAgenteService
{
    private AgenteSettings Cfg => opcoes.Value;

    private string CaminhoExe => Path.IsPathRooted(Cfg.CaminhoExe)
        ? Cfg.CaminhoExe
        : Path.Combine(ambiente.ContentRootPath, Cfg.CaminhoExe);

    public async Task<LinkAgenteDto> CriarLinkAsync(Guid autorId, Guid? funcionarioId, CancellationToken ct = default)
    {
        if (!File.Exists(CaminhoExe))
            throw new NotFoundException("O agente ainda não foi publicado nesta instalação (agente\\publicar.ps1).");

        var expira = DateTime.UtcNow.AddHours(Cfg.LinkTtlHoras);
        var token = CriarToken(expira);
        var caminho = $"/agente?t={token}";

        if (funcionarioId is { } destinatario)
        {
            if (!await users.ExistsAsync(destinatario, ct))
                throw new NotFoundException("Utilizador não encontrado.");

            var autor = await users.GetByIdAsync(autorId, ct);
            await notificacaoService.CriarAsync(
                destinatario,
                $"{autor?.Nome ?? "A equipa de TI"} enviou-lhe o Agente Novati, para o técnico poder ajudar no seu PC. Abra para descarregar.",
                caminho, ct);
            await uow.SaveChangesAsync(ct);
        }

        return new LinkAgenteDto(caminho, token, expira, funcionarioId);
    }

    public AgenteInfoDto Info(string? token)
    {
        var ficheiro = new FileInfo(CaminhoExe);
        var expira = LerToken(token);
        var valido = expira is { } e && e > DateTime.UtcNow;
        return new AgenteInfoDto(ficheiro.Exists, valido, expira, ficheiro.Exists ? ficheiro.Length : null);
    }

    public (string Caminho, byte[] Marca) ParaDownload(string? token, string servidor)
    {
        var expira = LerToken(token)
                     ?? throw new ForbiddenException("Link de download inválido.");
        if (expira <= DateTime.UtcNow)
            throw new ExpiradoException("Este link de download expirou. Peça um novo à equipa de TI.", "LINK_EXPIRADO");
        if (!File.Exists(CaminhoExe))
            throw new NotFoundException("O agente ainda não foi publicado nesta instalação.");

        var url = string.IsNullOrWhiteSpace(Cfg.UrlPublica) ? servidor : Cfg.UrlPublica;
        // O agente procura esta marca nos últimos bytes do próprio .exe (agente\Configuracao.cs).
        return (CaminhoExe, Encoding.ASCII.GetBytes($"\n<<NOVATI-SERVIDOR:{url.TrimEnd('/')}>>\n"));
    }

    // ─── Token do link ────────────────────────────────────
    // "<validade unix>.<HMAC-SHA256>": não precisa de tabela — o servidor reconhece os links que
    // ele próprio assinou e ninguém consegue alterar a validade sem conhecer a chave.

    private string CriarToken(DateTime expira)
    {
        var validade = new DateTimeOffset(expira).ToUnixTimeSeconds();
        return $"{validade}.{Assinar(validade)}";
    }

    /// <summary>A validade do link, ou null se o token não foi assinado por este servidor.</summary>
    private DateTime? LerToken(string? token)
    {
        var partes = (token ?? "").Split('.');
        if (partes.Length != 2 || !long.TryParse(partes[0], out var validade))
            return null;

        var certo = CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(partes[1]), Encoding.ASCII.GetBytes(Assinar(validade)));
        return certo ? DateTimeOffset.FromUnixTimeSeconds(validade).UtcDateTime : null;
    }

    private string Assinar(long validade)
        => Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(jwt.Value.Key), Encoding.UTF8.GetBytes($"agente-download:{validade}")));
}
