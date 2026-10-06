namespace Novati.API.Models.Entities;
/// <summary>Localização física (árvore: edifício → sala → posto).</summary>
public class Localizacao: BaseEntity
{
    public string Nome { get; set; } = "";

    // Auto-relação: pai
    public Guid? PaiId { get; set; }
    public Localizacao? Pai { get; set; }

    // Auto-relação: filhos
    public List<Localizacao> Filhos { get; set; } = [];
}
