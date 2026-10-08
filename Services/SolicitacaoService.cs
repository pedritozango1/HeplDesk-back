using Novati.API.Common.Exceptions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Solicitacoes;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Regras de negócio das solicitações de suporte.</summary>
public class SolicitacaoService(
    ISolicitacaoRepository solicitacoes,
    IFicheiroRepository ficheiros,
    IDispositivoRepository dispositivos,
    INotificacaoService notificacaoService,
    IUnitOfWork uow) : ISolicitacaoService
{
    public async Task<List<SolicitacaoDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default)
    {
        var lista = role == Role.FUNCIONARIO
            ? await solicitacoes.GetBySolicitanteAsync(userId, ct)
            : await solicitacoes.GetAllAsync(ct);

        return lista.Select(SolicitacaoMapper.ToDto).ToList();
    }

    public async Task<PaginaResultado<SolicitacaoDto>> GetPaginaAsync(Guid userId, Role role, PaginacaoQuery paginacao, string? estado, CancellationToken ct = default)
    {
        var filtroEstado = FiltroEnum.Ler<EstadoSolicitacao>(estado, "estado");

        // Mesma regra de visibilidade da lista completa: FUNCIONARIO só vê as suas.
        Guid? solicitanteId = role == Role.FUNCIONARIO ? userId : null;

        return (await solicitacoes.GetPaginaAsync(paginacao, filtroEstado, solicitanteId, ct)).Map(SolicitacaoMapper.ToDto);
    }

    public async Task<SolicitacaoDto> CreateAsync(Guid solicitanteId, Role role, CreateSolicitacaoRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<Prioridade>(request.Prioridade, out var prioridade))
            throw new BusinessRuleException("Prioridade inválida.");

        // Dispositivo com responsável é pessoal: só o próprio, um gestor ou um admin o reportam.
        // Sem responsável é da sala (ex.: impressora) — qualquer utilizador pode reportar.
        Guid? avisarResponsavelId = null;
        string patrimonio = "";
        if (request.DispositivoFisicoId is { } dispositivoId)
        {
            var dispositivo = await dispositivos.GetByIdAsync(dispositivoId, ct)
                               ?? throw new NotFoundException("Dispositivo não encontrado.");

            if (dispositivo.ResponsavelId is { } responsavelId && responsavelId != solicitanteId)
            {
                if (role is not (Role.ADMIN or Role.GESTOR))
                    throw new ForbiddenException("Este equipamento está atribuído a outro funcionário. Só ele, um gestor ou um administrador podem pedir a sua reparação.");

                avisarResponsavelId = responsavelId;
                patrimonio = dispositivo.Patrimonio;
            }
        }

        // Nome e tipo dos anexos vêm do Ficheiro (detetados pelo servidor), não do request.
        var ids = request.Anexos.Select(a => a.FicheiroId).Distinct().ToList();
        // Sem "ficheiroId" no JSON o Guid fica vazio: é um pedido mal formado (400), não um ficheiro em falta (404).
        if (ids.Contains(Guid.Empty))
            throw new BusinessRuleException("Cada anexo tem de indicar o ficheiroId de um ficheiro carregado em POST /api/ficheiros.");
        var anexos = await ficheiros.GetByIdsAsync(ids, ct);
        if (anexos.Count != ids.Count)
            throw new NotFoundException("Um dos anexos não foi encontrado.");
        if (anexos.Any(f => f.CriadoPorId != solicitanteId))
            throw new ForbiddenException("Só pode anexar ficheiros carregados por si.");

        var solicitacao = SolicitacaoMapper.ToEntity(request, solicitanteId, prioridade, anexos);
        await solicitacoes.AddAsync(solicitacao, ct);

        // Mesma transação: se a notificação falhasse a gravar, a solicitação também não gravava.
        await notificacaoService.NotificarPerfisAsync(
            [Role.TECNICO], $"Nova solicitação: {solicitacao.Titulo}", "/atendimentos", ct);

        // Aberta por um gestor/admin sobre o equipamento de outra pessoa: o dono fica a saber.
        if (avisarResponsavelId is { } dono)
            await notificacaoService.CriarAsync(
                dono, $"Foi aberto um pedido de reparação para o seu equipamento {patrimonio}: {solicitacao.Titulo}", null, ct);

        await uow.SaveChangesAsync(ct);

        return SolicitacaoMapper.ToDto(solicitacao);
    }

    public async Task<SolicitacaoDto> FecharAsync(Guid id, Guid userId, Role role, CancellationToken ct = default)
    {
        var solicitacao = await solicitacoes.GetByIdAsync(id, ct)
                           ?? throw new NotFoundException("Solicitação não encontrada.");

        var podeFechar = solicitacao.SolicitanteId == userId || role is Role.GESTOR or Role.ADMIN;
        if (!podeFechar)
            throw new ForbiddenException("Sem permissão para fechar esta solicitação.");

        if (solicitacao.Estado != EstadoSolicitacao.RESOLVIDA)
            throw new BusinessRuleException("Só solicitações resolvidas podem ser fechadas.");

        solicitacao.Estado = EstadoSolicitacao.FECHADA;
        solicitacoes.Update(solicitacao);
        await uow.SaveChangesAsync(ct);

        return SolicitacaoMapper.ToDto(solicitacao);
    }

    public async Task<SolicitacaoDto> AvaliarAsync(Guid id, Guid userId, AvaliarSolicitacaoRequest request, CancellationToken ct = default)
    {
        var solicitacao = await solicitacoes.GetByIdAsync(id, ct)
                           ?? throw new NotFoundException("Solicitação não encontrada.");

        if (solicitacao.SolicitanteId != userId)
            throw new ForbiddenException("Só o solicitante pode avaliar.");

        if (solicitacao.Estado is not (EstadoSolicitacao.RESOLVIDA or EstadoSolicitacao.FECHADA))
            throw new BusinessRuleException("Só se pode avaliar uma solicitação resolvida ou fechada.");

        if (solicitacao.Avaliacao is not null)
            throw new BusinessRuleException("Esta solicitação já foi avaliada.");

        solicitacao.Avaliacao = new Avaliacao
        {
            Estrelas = request.Estrelas,
            Comentario = request.Comentario,
            Data = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        solicitacoes.Update(solicitacao);
        await uow.SaveChangesAsync(ct);

        return SolicitacaoMapper.ToDto(solicitacao);
    }

    public async Task<SolicitacaoDto> ResolverViaBaseAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var solicitacao = await solicitacoes.GetByIdAsync(id, ct)
                           ?? throw new NotFoundException("Solicitação não encontrada.");

        if (solicitacao.SolicitanteId != userId)
            throw new ForbiddenException("Só o solicitante pode resolver via base de conhecimento.");

        if (solicitacao.Estado != EstadoSolicitacao.ABERTA)
            throw new BusinessRuleException("Só solicitações abertas podem ser resolvidas via base de conhecimento.");

        solicitacao.Estado = EstadoSolicitacao.RESOLVIDA;
        solicitacao.ResolvidaViaBase = true;

        solicitacoes.Update(solicitacao);
        await uow.SaveChangesAsync(ct);

        return SolicitacaoMapper.ToDto(solicitacao);
    }
}
