using Novati.API.Dtos.Notificacoes;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface INotificacaoService
{
    /// <summary>Só adiciona ao contexto — quem chama grava tudo numa só transação (SaveChanges próprio).</summary>
    Task CriarAsync(Guid userId, string message, string? link, CancellationToken ct = default);

    /// <summary>Cria uma notificação para todos os utilizadores com algum destes perfis.</summary>
    Task NotificarPerfisAsync(Role[] roles, string message, string? link, CancellationToken ct = default);

    Task<List<NotificacaoDto>> GetMinhasAsync(Guid userId, CancellationToken ct = default);
    Task MarcarLidaAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task MarcarTodasLidasAsync(Guid userId, CancellationToken ct = default);
}
