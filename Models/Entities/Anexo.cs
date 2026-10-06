namespace Novati.API.Models.Entities;
/// <summary>Anexo associado a uma solicitação. Guardado como JSONB.</summary>
public class Anexo
{
    /// <summary>Ficheiro carregado em POST /api/ficheiros (anexos novos).</summary>
    public Guid? FicheiroId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Legado: anexos antigos guardavam o conteúdo em base64 aqui. Os novos deixam null.</summary>
    public string? DataUrl { get; set; }
}
