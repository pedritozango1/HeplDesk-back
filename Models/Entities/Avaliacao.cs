namespace Novati.API.Models.Entities;
/// <summary>Avaliação do solicitante sobre uma solicitação resolvida. Owned type.</summary>
public class Avaliacao
{
    public int Estrelas { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateOnly Data { get; set; } 
}
