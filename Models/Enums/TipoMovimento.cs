namespace Novati.API.Models.Enums;

/// <summary>
/// Tipo de um <c>MovimentoStock</c>, o registo de auditoria de quantidade de um <c>ItemStock</c>.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum TipoMovimento
{
    /// <summary>Entrada de novas unidades em stock (compra recebida).</summary>
    ENTRADA,

    /// <summary>Saída de unidades do stock.</summary>
    SAIDA,

    /// <summary>Movimento entre localizações/depósitos.</summary>
    TRANSFERENCIA,

    /// <summary>Unidade marcada como reservada para uma ordem de reparação.</summary>
    RESERVA,

    /// <summary>Unidade instalada num dispositivo, saindo definitivamente do stock disponível.</summary>
    INSTALACAO,
}
