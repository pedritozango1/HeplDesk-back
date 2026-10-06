namespace Novati.API.Models.Entities;

/// <summary>Notificação enviada a um utilizador.</summary>
public class Notificacao : BaseEntity
{
    public string Message { get; set; } = "";
    public string? Link { get; set; }
    public bool Lida { get; set; }
    public DateOnly Data { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
