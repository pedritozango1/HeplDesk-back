namespace Novati.API.Dtos.Chat;

/// <summary>
/// Um bloco do histórico de uma conversa, por ordem cronológica.
/// <paramref name="HaMais"/> diz ao front se ainda há mensagens mais antigas (botão "ver mais").
/// </summary>
public record MensagensPaginaDto(IReadOnlyList<MensagemDto> Itens, bool HaMais);
