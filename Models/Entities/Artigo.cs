namespace Novati.API.Models.Entities;

/// <summary>Artigo da base de conhecimento.</summary>
public class Artigo : BaseEntity
{
    public string Titulo { get; set; } = "";
    public string Conteudo { get; set; } = "";
    public string Categoria { get; set; } = "";

    // Lista de strings → text[] no Postgres
    public List<string> Tags { get; set; } = [];

    public Guid AutorId { get; set; }
    public User Autor { get; set; } = null!;

    /// <summary>Avaliações "Isto ajudou?" dos utilizadores.</summary>
    public List<ArtigoAvaliacao> Avaliacoes { get; set; } = []; 
}
