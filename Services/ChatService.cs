using Novati.API.Common.Exceptions;
using Novati.API.Dtos.Chat;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Chat: cada conversa corresponde a uma solicitação e só envolve quem nela participa.</summary>
public class ChatService(
    IMensagemRepository mensagens,
    ISolicitacaoRepository solicitacoes,
    IOrdemRepository ordens,
    INotificacaoService notificacaoService,
    IUnitOfWork uow) : IChatService
{
    public async Task<List<ConversaResumoDto>> GetConversasAsync(Guid userId, Role role, CancellationToken ct = default)
    {
        // Mesma visibilidade de utils/chat.js no front: funcionário vê as suas solicitações,
        // técnico vê as ordens que assumiu, gestor/admin veem tudo (null = sem filtro).
        IReadOnlyCollection<Guid>? ids = null;

        if (role == Role.FUNCIONARIO)
            ids = [.. (await solicitacoes.GetBySolicitanteAsync(userId, ct)).Select(s => s.Id)];
        else if (role is not (Role.ADMIN or Role.GESTOR))
            ids = [.. await ordens.GetSolicitacaoIdsPorTecnicoAsync(userId, ct)];

        var naoLidas = await mensagens.ContarNaoLidasAsync(userId, ids, ct);
        var ultimas = await mensagens.GetUltimasAsync(ids, ct);

        return ultimas
            .Select(m => new ConversaResumoDto(m.SolicitacaoId, naoLidas.GetValueOrDefault(m.SolicitacaoId), MensagemMapper.ToDto(m)))
            .ToList();
    }

    public async Task<MensagensPaginaDto> GetMensagensDaConversaAsync(Guid userId, Role role, Guid solicitacaoId, MensagensQuery query, CancellationToken ct = default)
    {
        var solicitacao = await solicitacoes.GetByIdAsync(solicitacaoId, ct)
                         ?? throw new NotFoundException("Solicitação não encontrada.");

        // Leitura: sem acesso devolve vazio em vez de 403 (como as outras listagens do chat).
        if (!await TemAcessoAsync(userId, role, solicitacao, ct))
            return new MensagensPaginaDto([], false);

        // Pede uma a mais só para saber se ainda há mensagens mais antigas.
        var bloco = await mensagens.GetRecentesAsync(solicitacaoId, query.Saltar, query.Limite + 1, ct);
        var haMais = bloco.Count > query.Limite;

        // Vêm da mais recente para a mais antiga; o front mostra por ordem cronológica.
        var itens = bloco.Take(query.Limite).Reverse().Select(MensagemMapper.ToDto).ToList();
        return new MensagensPaginaDto(itens, haMais);
    }

    public async Task<MensagemDto> CriarAsync(Guid autorId, Role role, CreateMensagemRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
            throw new BusinessRuleException("O texto da mensagem é obrigatório.");

        var solicitacao = await solicitacoes.GetByIdAsync(request.SolicitacaoId, ct)
                         ?? throw new NotFoundException("Solicitação não encontrada.");

        await GarantirAcessoAsync(autorId, role, solicitacao, ct);

        var mensagem = MensagemMapper.ToEntity(request, autorId);
        await mensagens.AddAsync(mensagem, ct);

        // Notifica a outra parte da conversa (link /chat).
        if (solicitacao.SolicitanteId != autorId)
            await notificacaoService.CriarAsync(
                solicitacao.SolicitanteId,
                $"Nova mensagem na solicitação \"{solicitacao.Titulo}\".",
                "/chat", ct);
        else
        {
            var tecnicoId = await ordens.GetTecnicoIdPorSolicitacaoAsync(solicitacao.Id, ct);
            if (tecnicoId is not null)
                await notificacaoService.CriarAsync(
                    tecnicoId.Value,
                    $"Nova mensagem na solicitação \"{solicitacao.Titulo}\".",
                    "/chat", ct);
        }

        await uow.SaveChangesAsync(ct);

        return MensagemMapper.ToDto(mensagem);
    }

    public async Task MarcarConversaLidaAsync(Guid userId, Guid solicitacaoId, CancellationToken ct = default)
    {
        // Só as mensagens de outros contam como lidas — as próprias já foram lidas ao enviar.
        // O filtro é feito na BD: não se carrega a conversa inteira para marcar duas ou três.
        var porLer = await mensagens.GetPorLerAsync(solicitacaoId, userId, ct);
        if (porLer.Count == 0)
            return;

        foreach (var mensagem in porLer)
        {
            mensagem.Lida = true;
            mensagens.Update(mensagem);
        }

        await uow.SaveChangesAsync(ct);
    }

    private async Task GarantirAcessoAsync(Guid userId, Role role, Solicitacao solicitacao, CancellationToken ct)
    {
        if (await TemAcessoAsync(userId, role, solicitacao, ct))
            return;

        throw new ForbiddenException(role == Role.FUNCIONARIO
            ? "Não tem acesso a esta conversa."
            : "Só conversa com quem tem uma ordem sobre esta solicitação.");
    }

    private async Task<bool> TemAcessoAsync(Guid userId, Role role, Solicitacao solicitacao, CancellationToken ct)
    {
        if (role is Role.ADMIN or Role.GESTOR)
            return true;

        if (role == Role.FUNCIONARIO)
            return solicitacao.SolicitanteId == userId;

        return await ordens.GetTecnicoIdPorSolicitacaoAsync(solicitacao.Id, ct) == userId;
    }
}
