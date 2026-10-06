namespace Novati.API.Dtos.Chat;

/// <summary>Mensagem de chat associada a uma solicitação.</summary>
public record MensagemDto(Guid Id, Guid SolicitacaoId, Guid AutorId, string Texto, DateOnly Data, string Hora, bool Lida);
