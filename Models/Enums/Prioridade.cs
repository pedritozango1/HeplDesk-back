namespace Novati.API.Models.Enums;

/// <summary>
/// Nível de urgência atribuído a uma <c>Solicitacao</c> pelo solicitante ou pela equipa técnica.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum Prioridade
{
    BAIXA,
    MEDIA,
    ALTA,
    URGENTE,
}
