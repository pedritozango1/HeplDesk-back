namespace Novati.API.Models.Enums;

/// <summary>
/// Estado de um pedido de acesso remoto ao PC do solicitante.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum EstadoSessaoRemota
{
    /// <summary>O técnico pediu acesso; aguarda a resposta do solicitante.</summary>
    PEDIDA,

    /// <summary>O solicitante autorizou; existe um token por resgatar.</summary>
    AUTORIZADA,

    /// <summary>O técnico resgatou o token; a ligação entre os dois PCs está permitida.</summary>
    ATIVA,

    /// <summary>Sessão ativa que chegou ao fim (por uma das partes ou por tempo máximo).</summary>
    TERMINADA,

    /// <summary>O solicitante recusou o pedido.</summary>
    RECUSADA,

    /// <summary>Ninguém respondeu a tempo, o token caducou ou esgotaram-se as tentativas.</summary>
    EXPIRADA,

    /// <summary>Desistência antes de a sessão começar.</summary>
    CANCELADA,
}
