namespace Novati.API.Models.Entities;
/// <summary>Peça usada num relatório técnico. Guardado como JSONB.</summary>
public class PecaRelatorio
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string Codigo { get; set; } = string.Empty;
    
}
