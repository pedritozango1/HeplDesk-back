namespace Novati.API.Dtos.Chat;

/// <summary>
/// Resumo de uma conversa para a lista do chat: quantas mensagens tem por ler e qual foi a última.
/// Evita trazer o histórico inteiro só para desenhar a lista e o contador.
/// </summary>
public record ConversaResumoDto(Guid SolicitacaoId, int NaoLidas, MensagemDto Ultima);
