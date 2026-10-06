namespace Novati.API.Models.Enums;

/// <summary>
/// Estado de uma <c>UnidadeStock</c> (uma peça física individual em stock).
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum EstadoUnidade
{
    /// <summary>Em stock, livre para reserva.</summary>
    DISPONIVEL,

    /// <summary>Reservada para uma ordem de reparação específica (<c>ReservadaParaOrdemId</c>).</summary>
    RESERVADA,

    /// <summary>Inutilizável, retirada de circulação.</summary>
    AVARIADA,

    /// <summary>Já instalada num dispositivo (deixou de estar em stock disponível).</summary>
    INSTALADA,
}
