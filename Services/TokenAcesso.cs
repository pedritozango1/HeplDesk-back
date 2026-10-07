using System.Security.Cryptography;
using System.Text;

namespace Novati.API.Services;

/// <summary>
/// Token de uso único do acesso remoto: geração, normalização e verificação.
/// Classe pura (sem BD nem configuração) — toda a criptografia do módulo está aqui.
/// </summary>
public static class TokenAcesso
{
    public const int Tamanho = 8;

    // Sem 0/O, 1/I/L: o solicitante pode ter de ditar o código ao telefone.
    private const string Alfabeto = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    /// <summary>Novo código aleatório (gerador criptográfico — System.Random seria previsível).</summary>
    public static string Gerar() => RandomNumberGenerator.GetString(Alfabeto, Tamanho);

    /// <summary>Como se mostra a uma pessoa: XXXX-XXXX.</summary>
    public static string Formatar(string token) => $"{token[..4]}-{token[4..]}";

    /// <summary>Tira hífenes e espaços e põe em maiúsculas: "abcd-efgh" e "ABCDEFGH" são o mesmo código.</summary>
    public static string Normalizar(string? texto)
        => new((texto ?? "").Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    /// <summary>
    /// SHA-256 em hexadecimal — é isto que vai para a BD. O Id da sessão entra na conta
    /// para o mesmo código dar hashes diferentes em sessões diferentes.
    /// </summary>
    public static string Hash(Guid sessaoId, string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{sessaoId:N}:{token}")));

    /// <summary>Compara em tempo constante: a duração não revela quantos caracteres estavam certos.</summary>
    public static bool Verificar(Guid sessaoId, string token, string? hashGuardado)
        => hashGuardado is not null
           && CryptographicOperations.FixedTimeEquals(
               Encoding.ASCII.GetBytes(Hash(sessaoId, token)),
               Encoding.ASCII.GetBytes(hashGuardado));
}
