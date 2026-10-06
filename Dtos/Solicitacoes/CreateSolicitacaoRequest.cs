using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Solicitacoes;

/// <summary>Pedido de criação de uma solicitação de suporte.</summary>
public class CreateSolicitacaoRequest
{
    /// <example>Laptop não liga</example>
    [Required]
    public string Titulo { get; set; } = "";

    /// <example>O portátil não liga mesmo com o carregador ligado.</example>
    [Required]
    public string Descricao { get; set; } = "";

    /// <summary>Dispositivo relacionado, se aplicável.</summary>
    public Guid? DispositivoFisicoId { get; set; }

    /// <summary>BAIXA, MEDIA, ALTA ou URGENTE.</summary>
    /// <example>ALTA</example>
    [Required]
    public string Prioridade { get; set; } = "";

    /// <example>Hardware</example>
    [Required]
    public string Categoria { get; set; } = "";

    /// <summary>Ficheiros já carregados em POST /api/ficheiros.</summary>
    public List<AnexoRequest> Anexos { get; set; } = [];
}
