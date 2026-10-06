using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Settings;
using Novati.API.Dtos.Ficheiros;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>
/// Upload/download de ficheiros. Regras: tamanho máximo, só tipos conhecidos e o tipo
/// é detetado pelos primeiros bytes (assinatura "magic number") — o Content-Type e a
/// extensão enviados pelo cliente não são de confiança.
/// </summary>
public class FicheiroService(
    IFicheiroRepository ficheiros,
    IFileStorage storage,
    IOptions<StorageSettings> options,
    IUnitOfWork uow,
    ILogger<FicheiroService> logger) : IFicheiroService
{
    private readonly StorageSettings _settings = options.Value;

    public async Task<FicheiroDto> UploadAsync(Guid userId, IFormFile ficheiro, CancellationToken ct = default)
    {
        if (ficheiro.Length == 0)
            throw new BusinessRuleException("O ficheiro está vazio.");
        if (ficheiro.Length > _settings.MaxBytes)
            throw new BusinessRuleException($"O ficheiro excede o limite de {_settings.MaxBytes / (1024 * 1024)} MB.");

        // Até 10 MB cabe em memória: precisamos dos bytes para detetar o tipo e calcular o hash.
        using var memoria = new MemoryStream((int)ficheiro.Length);
        await ficheiro.CopyToAsync(memoria, ct);
        var bytes = memoria.ToArray();

        var nome = NomeSeguro(ficheiro.FileName);
        var (contentType, extensao) = DetetarTipo(bytes, Path.GetExtension(nome))
            ?? throw new BusinessRuleException("Tipo de ficheiro não suportado. Permitidos: PNG, JPEG, GIF, WEBP, PDF, DOCX, XLSX e TXT.");

        memoria.Position = 0;
        var caminho = await storage.GuardarAsync(memoria, extensao, ct);

        var entidade = new Ficheiro
        {
            NomeOriginal = nome,
            ContentType = contentType,
            TamanhoBytes = bytes.LongLength,
            CaminhoRelativo = caminho,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = userId,
        };

        try
        {
            await ficheiros.AddAsync(entidade, ct);
            await uow.SaveChangesAsync(ct);
        }
        catch
        {
            // Disco e BD não partilham transação: se a BD falhar, não deixar o ficheiro órfão.
            storage.Apagar(caminho);
            throw;
        }

        return FicheiroMapper.ToDto(entidade);
    }

    public async Task<FicheiroDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => FicheiroMapper.ToDto(await CarregarAsync(id, ct));

    public async Task<(Stream Conteudo, string ContentType, string Nome)> AbrirAsync(Guid id, CancellationToken ct = default)
    {
        var f = await CarregarAsync(id, ct);
        return (storage.Abrir(f.CaminhoRelativo), f.ContentType, f.NomeOriginal);
    }

    public async Task DeleteAsync(Guid userId, Role role, Guid id, CancellationToken ct = default)
    {
        var f = await CarregarAsync(id, ct);

        if (f.CriadoPorId != userId && role != Role.ADMIN)
            throw new ForbiddenException("Só quem carregou o ficheiro (ou um ADMIN) o pode apagar.");

        if (await ficheiros.EstaEmUsoAsync(id, ct))
            throw new ConflictException("O ficheiro está em uso (assinatura, relatório ou anexo) e não pode ser apagado.");

        ficheiros.Remove(f);
        await uow.SaveChangesAsync(ct);

        // Só depois de a BD confirmar: se o disco falhar fica um ficheiro órfão (inofensivo),
        // nunca uma linha na BD sem conteúdo.
        try { storage.Apagar(f.CaminhoRelativo); }
        catch (Exception ex) { logger.LogWarning(ex, "Não foi possível apagar {Caminho} do armazenamento", f.CaminhoRelativo); }
    }

    // ─── Auxiliares ───────────────────────────────────────

    private async Task<Ficheiro> CarregarAsync(Guid id, CancellationToken ct)
        => await ficheiros.GetByIdAsync(id, ct)
           ?? throw new NotFoundException("Ficheiro não encontrado.");

    /// <summary>Tira pastas e caracteres de controlo do nome enviado; limita o comprimento.</summary>
    private static string NomeSeguro(string? nome)
    {
        var limpo = new string(Path.GetFileName(nome ?? "").Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (limpo.Length == 0) limpo = "ficheiro";
        return limpo.Length <= 255 ? limpo : limpo[^255..];
    }

    /// <summary>Tipo real a partir dos primeiros bytes. Devolve null se não for um tipo permitido.</summary>
    private static (string ContentType, string Extensao)? DetetarTipo(byte[] b, string extensaoCliente)
    {
        var ext = extensaoCliente.ToLowerInvariant();

        if (Comeca(b, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])) return ("image/png", ".png");
        if (Comeca(b, [0xFF, 0xD8, 0xFF])) return ("image/jpeg", ".jpg");
        if (Comeca(b, "GIF87a"u8) || Comeca(b, "GIF89a"u8)) return ("image/gif", ".gif");
        if (Comeca(b, "RIFF"u8) && b.Length >= 12 && b.AsSpan(8, 4).SequenceEqual("WEBP"u8)) return ("image/webp", ".webp");
        if (Comeca(b, "%PDF-"u8)) return ("application/pdf", ".pdf");

        // DOCX/XLSX são ZIPs: os bytes só dizem "zip"; a extensão escolhe entre os dois formatos Office.
        if (Comeca(b, [0x50, 0x4B, 0x03, 0x04]))
        {
            if (ext == ".docx") return ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");
            if (ext == ".xlsx") return ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx");
            return null;   // outros ZIPs não
        }

        // Texto não tem assinatura: aceita .txt se for UTF-8 válido e sem bytes nulos.
        if (ext == ".txt" && !b.Contains((byte)0))
        {
            try
            {
                _ = new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(b);
                return ("text/plain; charset=utf-8", ".txt");
            }
            catch (DecoderFallbackException) { return null; }
        }

        return null;
    }

    private static bool Comeca(byte[] b, ReadOnlySpan<byte> prefixo)
        => b.AsSpan().StartsWith(prefixo);
}
