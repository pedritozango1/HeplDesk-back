using Novati.API.Common.Exceptions;
using Novati.API.Dtos.Notificacoes;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Sistema de notificações, usado internamente por todos os módulos a partir daqui.</summary>
public class NotificacaoService(
    INotificacaoRepository notificacoes,
    IUserRepository users,
    IUnitOfWork uow) : INotificacaoService
{
    public async Task CriarAsync(Guid userId, string message, string? link, CancellationToken ct = default)
    {
        var notificacao = new Notificacao
        {
            UserId = userId,
            Message = message,
            Link = link,
            Lida = false,
            Data = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        // Sem SaveChangesAsync aqui de propósito: quem chama grava tudo numa só transação.
        await notificacoes.AddAsync(notificacao, ct);
    }

    public async Task NotificarPerfisAsync(Role[] roles, string message, string? link, CancellationToken ct = default)
    {
        var todos = await users.GetAllAsync(ct);
        foreach (var user in todos.Where(u => roles.Contains(u.Role)))
            await CriarAsync(user.Id, message, link, ct);
    }

    public async Task<List<NotificacaoDto>> GetMinhasAsync(Guid userId, CancellationToken ct = default)
        => (await notificacoes.GetByUserOrderedAsync(userId, ct)).Select(NotificacaoMapper.ToDto).ToList();

    public async Task MarcarLidaAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var notificacao = await notificacoes.GetByIdAsync(id, ct)
                           ?? throw new NotFoundException("Notificação não encontrada.");

        if (notificacao.UserId != userId)
            throw new ForbiddenException("Esta notificação não é tua.");

        notificacao.Lida = true;
        notificacoes.Update(notificacao);
        await uow.SaveChangesAsync(ct);
    }

    public async Task MarcarTodasLidasAsync(Guid userId, CancellationToken ct = default)
    {
        var minhas = await notificacoes.GetByUserOrderedAsync(userId, ct);
        foreach (var notificacao in minhas.Where(n => !n.Lida))
        {
            notificacao.Lida = true;
            notificacoes.Update(notificacao);
        }

        await uow.SaveChangesAsync(ct);
    }
}
