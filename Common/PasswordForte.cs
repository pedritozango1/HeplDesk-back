namespace Novati.API.Common;

/// <summary>
/// Regra de password forte. As mesmas condições são mostradas ao utilizador no perfil
/// (novati/src/utils/password.js) — o servidor é quem decide, o front só antecipa.
/// </summary>
public static class PasswordForte
{
    public const int ComprimentoMinimo = 8;

    /// <summary>O que falta à password para ser forte; lista vazia = é forte.</summary>
    public static List<string> Falhas(string password)
    {
        var falhas = new List<string>();

        if (password.Length < ComprimentoMinimo) falhas.Add($"pelo menos {ComprimentoMinimo} caracteres");
        if (!password.Any(char.IsUpper)) falhas.Add("uma letra maiúscula");
        if (!password.Any(char.IsLower)) falhas.Add("uma letra minúscula");
        if (!password.Any(char.IsDigit)) falhas.Add("um número");
        if (password.All(char.IsLetterOrDigit)) falhas.Add("um símbolo (ex.: ! # $ %)");

        return falhas;
    }
}
