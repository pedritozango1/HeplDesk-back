using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Chat;

/// <summary>
/// Parâmetros do histórico de uma conversa (?saltar=30&amp;limite=30). Conta-se a partir da
/// mensagem mais recente: saltar=0 devolve as últimas, saltar=30 as 30 anteriores a essas.
/// </summary>
public class MensagensQuery
{
    /// <summary>Teto do bloco: impede que um cliente peça a conversa inteira de uma vez.</summary>
    public const int LimiteMaximo = 100;

    /// <summary>Quantas mensagens (das mais recentes) o cliente já tem.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "O valor de saltar tem de ser 0 ou superior.")]
    public int Saltar { get; set; }

    /// <summary>Quantas mensagens devolver (1 a 100).</summary>
    [Range(1, LimiteMaximo, ErrorMessage = "O limite tem de estar entre 1 e 100.")]
    public int Limite { get; set; } = 30;
}
