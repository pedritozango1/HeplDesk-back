using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.AcessoRemoto;

/// <summary>Pedido de acesso remoto feito pelo técnico.</summary>
public class PedirAcessoRequest
{
    /// <example>VER</example>
    public string Modo { get; set; } = "VER";
}

/// <summary>Autorização do solicitante. No modo CONTROLAR leva o código que o Agente Novati mostra no PC.</summary>
public class AutorizarRequest
{
    /// <example>482 913 057</example>
    [MaxLength(20, ErrorMessage = "O código do agente é demasiado longo.")]
    public string? CodigoAgente { get; set; }
}

/// <summary>Resgate do token de uso único.</summary>
public class ResgatarRequest
{
    /// <example>K7QM-2XRD</example>
    [Required(ErrorMessage = "O código é obrigatório.")]
    public string Token { get; set; } = "";
}

/// <summary>Motivo opcional de uma recusa ou de um fim de sessão.</summary>
public class MotivoRequest
{
    /// <example>Estou numa reunião, tente daqui a 10 minutos.</example>
    [MaxLength(200, ErrorMessage = "O motivo tem no máximo 200 caracteres.")]
    public string? Motivo { get; set; }
}
