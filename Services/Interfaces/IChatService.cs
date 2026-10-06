using Novati.API.Dtos.Chat;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Resumo (por ler + última mensagem) das conversas visíveis que já têm mensagens.
    /// FUNCIONARIO vê as suas conversas, TECNICO as ordens que assumiu, ADMIN/GESTOR todas.
    /// </summary>
    Task<List<ConversaResumoDto>> GetConversasAsync(Guid userId, Role role, CancellationToken ct = default);

    /// <summary>Um bloco do histórico de uma conversa, a contar da mensagem mais recente.</summary>
    Task<MensagensPaginaDto> GetMensagensDaConversaAsync(Guid userId, Role role, Guid solicitacaoId, MensagensQuery query, CancellationToken ct = default);

    Task<MensagemDto> CriarAsync(Guid autorId, Role role, CreateMensagemRequest request, CancellationToken ct = default);

    /// <summary>Marca como lidas as mensagens de outros nesta conversa.</summary>
    Task MarcarConversaLidaAsync(Guid userId, Guid solicitacaoId, CancellationToken ct = default);
}
