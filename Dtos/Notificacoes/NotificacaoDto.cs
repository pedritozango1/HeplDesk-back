namespace Novati.API.Dtos.Notificacoes;

/// <summary>Notificação enviada a um utilizador.</summary>
public record NotificacaoDto(Guid Id, Guid UserId, string Message, string? Link, bool Lida, DateOnly Data);
