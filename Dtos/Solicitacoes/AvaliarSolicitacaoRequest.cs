using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Solicitacoes;

/// <summary>Avaliação do solicitante a uma solicitação resolvida/fechada.</summary>
public class AvaliarSolicitacaoRequest
{
    /// <example>5</example>
    [Range(1, 5)]
    public int Estrelas { get; set; }

    /// <example>Rápido e eficiente.</example>
    public string Comentario { get; set; } = "";
}
