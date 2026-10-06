namespace Novati.API.Models.Entities;

/// <summary>
/// "Isto ajudou?" — avaliação de um artigo por um utilizador. Uma por utilizador e
/// artigo (votar outra vez substitui a anterior). O motor de sugestões usa o saldo
/// de avaliações para ordenar os artigos.
/// </summary>
public class ArtigoAvaliacao : BaseEntity
{
    public bool Util { get; set; }
    public DateOnly Data { get; set; }

    public Guid ArtigoId { get; set; }
    public Artigo Artigo { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
