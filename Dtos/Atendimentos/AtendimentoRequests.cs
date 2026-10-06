using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Atendimentos;

/// <summary>Registo do diagnóstico da ordem. Só aceite em EM_DIAGNOSTICO.</summary>
public class DiagnosticoRequest
{
    /// <example>Fonte de alimentação com falha de energia.</example>
    [Required]
    public string Diagnostico { get; set; } = "";
}

/// <summary>Comentário livre acrescentado ao histórico da ordem.</summary>
public class ComentarioRequest
{
    /// <example>Peça pedida ao fornecedor, retorno amanhã.</example>
    [Required]
    public string Texto { get; set; } = "";
}

/// <summary>Reatribuição da ordem a outro técnico (ADMIN/GESTOR).</summary>
public class ReatribuirRequest
{
    [Required]
    public Guid NovoTecnicoId { get; set; }
}

/// <summary>Reserva de uma peça pelo modelo de componente.</summary>
public class ReservarPecaRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid ModeloComponenteId { get; set; }
}

/// <summary>Instalação de uma unidade reservada num dispositivo físico.</summary>
public class InstalarPecaRequest
{
    [Required]
    public Guid UnidadeStockId { get; set; }

    [Required]
    public Guid DispositivoFisicoId { get; set; }

    /// <summary>Instância a substituir (opcional). Tem de pertencer ao mesmo dispositivo.</summary>
    public Guid? InstanciaAntigaId { get; set; }
}

/// <summary>
/// Proposta de solução ao solicitante. Não leva tempo gasto: é o servidor que cronometra
/// a ordem (de assumir até à aprovação), para o valor não poder ser inventado.
/// </summary>
public class PropostaSolucaoRequest
{
    /// <example>Fonte de alimentação substituída e equipamento testado.</example>
    [Required]
    public string Solucao { get; set; } = "";
}

/// <summary>Resposta do solicitante à proposta de solução.</summary>
public class ResponderValidacaoRequest
{
    public bool Aceite { get; set; }

    /// <summary>Obrigatório quando <c>aceite = false</c>.</summary>
    public string Motivo { get; set; } = "";
}

/// <summary>Solução sugerida pelo assistente de IA, registada na ordem.</summary>
public class SolucaoSugeridaRequest
{
    [Required]
    public string Solucao { get; set; } = "";
}
