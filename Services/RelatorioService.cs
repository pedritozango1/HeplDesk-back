using System.Text.Json;
using Novati.API.Common.Exceptions;
using Novati.API.Dtos.RelatoriosTecnicos;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>
/// Relatórios técnicos: documento formal de uma ordem resolvida, com máquina de estados
/// rascunho → finalizado → aprovado e auditoria de edições campo a campo.
/// </summary>
public class RelatorioService(
    IRelatorioRepository relatorios,
    IOrdemRepository ordens,
    ISolicitacaoRepository solicitacoes,
    IUserRepository users,
    INotificacaoService notificacaoService,
    IUnitOfWork uow) : IRelatorioService
{
    private static readonly StatusRelatorio[] EstadosVisiveisAoGestor = [StatusRelatorio.finalizado, StatusRelatorio.aprovado];

    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);

    // ─── Leitura ──────────────────────────────────────────

    public async Task<List<RelatorioDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default)
    {
        // Regra de ouro nº 4: a leitura filtra, nunca devolve 403 nem 404.
        StatusRelatorio[]? estados = null;
        IEnumerable<Guid>? minhasSolicitacoes = null;

        if (role == Role.GESTOR)
        {
            estados = EstadosVisiveisAoGestor;
        }
        else if (role == Role.FUNCIONARIO)
        {
            estados = [StatusRelatorio.aprovado];
            var minhas = await solicitacoes.GetBySolicitanteAsync(userId, ct);
            minhasSolicitacoes = [.. minhas.Select(s => s.Id)];
        }

        return (await relatorios.GetVisiveisAsync(estados, minhasSolicitacoes, ct))
            .Select(RelatorioMapper.ToDto).ToList();
    }

    public async Task<RelatorioDto> GetByIdAsync(Guid userId, Role role, Guid id, CancellationToken ct = default)
    {
        var relatorio = await CarregarAsync(id, ct);
        await GarantirLeituraAsync(userId, role, relatorio, ct);
        return RelatorioMapper.ToDto(relatorio);
    }

    // ─── Escrita ──────────────────────────────────────────

    public async Task<RelatorioDto> CreateAsync(Guid userId, Role role, CreateRelatorioRequest request, CancellationToken ct = default)
    {
        var ordem = await ordens.GetByIdAsync(request.OrdemId, ct)
                    ?? throw new NotFoundException("Ordem não encontrada.");

        if (ordem.TecnicoId != userId && role != Role.ADMIN)
            throw new ForbiddenException("Só o técnico que fez a reparação (ou um ADMIN) pode criar o relatório.");

        if (ordem.Estado != EstadoOrdem.RESOLVIDO)
            throw new BusinessRuleException("Só ordens resolvidas podem ter relatório técnico.");

        if (await ordens.GetRelatorioIdAsync(ordem.Id, ct) is not null)
            throw new ConflictException("Esta ordem já tem relatório técnico.");

        var relatorio = new RelatorioTecnico
        {
            OrdemId = ordem.Id,
            AutorId = userId,
            CriadoEm = Hoje,
            AtualizadoEm = Hoje,
            Status = StatusRelatorio.rascunho,
            Sumario = request.Sumario,
            Diagnostico = request.Diagnostico,
            SolucaoAplicada = request.SolucaoAplicada,
            PecasUsadas = [.. request.PecasUsadas.Select(p => new PecaRelatorio { Nome = p.Nome, Quantidade = p.Quantidade, Codigo = p.Codigo })],
            // Cronometrado pelo servidor na ordem — o relatório copia, não aceita outro valor.
            TempoGastoMin = ordem.TempoGastoMin,
            Procedimentos = request.Procedimentos,
            Observacoes = request.Observacoes,
            AssinaturaTecnico = request.AssinaturaTecnico,
            AssinaturaResponsavel = request.AssinaturaResponsavel,
            ComentariosInternos = request.ComentariosInternos,
        };

        AdicionarHistorico(relatorio, userId, AcaoRelatorio.CRIADO, "", "", "");
        await relatorios.AddAsync(relatorio, ct);
        await uow.SaveChangesAsync(ct);

        return RelatorioMapper.ToDto(relatorio);
    }

    public async Task<RelatorioDto> UpdateAsync(Guid userId, Role role, Guid id, UpdateRelatorioRequest request, CancellationToken ct = default)
    {
        var relatorio = await CarregarAsync(id, ct);
        GarantirEscrita(relatorio, userId, role);

        if (relatorio.Status == StatusRelatorio.aprovado)
            throw new BusinessRuleException("Um relatório aprovado não pode ser editado.");

        // Algoritmo de diff: um campo só entra no histórico se foi enviado E mudou de valor.
        if (request.Sumario is not null && request.Sumario != relatorio.Sumario)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "sumario", relatorio.Sumario, request.Sumario);
            relatorio.Sumario = request.Sumario;
        }

        if (request.Diagnostico is not null && request.Diagnostico != relatorio.Diagnostico)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "diagnostico", relatorio.Diagnostico, request.Diagnostico);
            relatorio.Diagnostico = request.Diagnostico;
        }

        if (request.SolucaoAplicada is not null && request.SolucaoAplicada != relatorio.SolucaoAplicada)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "solucaoAplicada", relatorio.SolucaoAplicada, request.SolucaoAplicada);
            relatorio.SolucaoAplicada = request.SolucaoAplicada;
        }

        if (request.PecasUsadas is not null)
        {
            var antes = SerializarPecas(relatorio.PecasUsadas);
            var depois = SerializarPecas([.. request.PecasUsadas.Select(p => new PecaRelatorio { Nome = p.Nome, Quantidade = p.Quantidade, Codigo = p.Codigo })]);

            if (antes != depois)
            {
                AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "pecasUsadas", antes, depois);
                relatorio.PecasUsadas = [.. request.PecasUsadas.Select(p => new PecaRelatorio { Nome = p.Nome, Quantidade = p.Quantidade, Codigo = p.Codigo })];
            }
        }

        // TempoGastoMin não é editável: vem do cronómetro da ordem (ver CreateAsync).

        if (request.Procedimentos is not null && request.Procedimentos != relatorio.Procedimentos)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "procedimentos", relatorio.Procedimentos, request.Procedimentos);
            relatorio.Procedimentos = request.Procedimentos;
        }

        if (request.Observacoes is not null && request.Observacoes != relatorio.Observacoes)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "observacoes", relatorio.Observacoes, request.Observacoes);
            relatorio.Observacoes = request.Observacoes;
        }

        if (request.AssinaturaTecnico is not null && request.AssinaturaTecnico != relatorio.AssinaturaTecnico)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "assinaturaTecnico", relatorio.AssinaturaTecnico, request.AssinaturaTecnico);
            relatorio.AssinaturaTecnico = request.AssinaturaTecnico;
        }

        if (request.AssinaturaResponsavel is not null && request.AssinaturaResponsavel != relatorio.AssinaturaResponsavel)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "assinaturaResponsavel", relatorio.AssinaturaResponsavel, request.AssinaturaResponsavel);
            relatorio.AssinaturaResponsavel = request.AssinaturaResponsavel;
        }

        if (request.ComentariosInternos is not null && request.ComentariosInternos != relatorio.ComentariosInternos)
        {
            AdicionarHistorico(relatorio, userId, AcaoRelatorio.EDITADO, "comentariosInternos", relatorio.ComentariosInternos, request.ComentariosInternos);
            relatorio.ComentariosInternos = request.ComentariosInternos;
        }

        relatorio.AtualizadoEm = Hoje;
        relatorios.Update(relatorio);
        await uow.SaveChangesAsync(ct);

        return await RecarregarAsync(id, ct);
    }

    public async Task<RelatorioDto> FinalizarAsync(Guid userId, Role role, Guid id, CancellationToken ct = default)
    {
        var relatorio = await CarregarAsync(id, ct);
        GarantirEscrita(relatorio, userId, role);

        if (relatorio.Status != StatusRelatorio.rascunho)
            throw new BusinessRuleException("Só um relatório em rascunho pode ser finalizado.");

        relatorio.Status = StatusRelatorio.finalizado;
        relatorio.AtualizadoEm = Hoje;

        // Assinar = congelar no documento a assinatura que o autor tem AGORA no perfil.
        var autor = await users.GetByIdAsync(relatorio.AutorId, ct);
        relatorio.AssinaturaTecnicoFicheiroId = autor?.AssinaturaFicheiroId;

        AdicionarHistorico(relatorio, userId, AcaoRelatorio.FINALIZADO, "", "", "");

        await notificacaoService.NotificarPerfisAsync(
            [Role.GESTOR],
            $"Relatório técnico finalizado para revisão: {relatorio.Ordem?.Solicitacao?.Titulo}.",
            $"/relatorios-tecnicos/{relatorio.Id}", ct);

        relatorios.Update(relatorio);
        await uow.SaveChangesAsync(ct);

        return await RecarregarAsync(id, ct);
    }

    public async Task<RelatorioDto> ReabrirAsync(Guid userId, Role role, Guid id, CancellationToken ct = default)
    {
        var relatorio = await CarregarAsync(id, ct);
        GarantirEscrita(relatorio, userId, role);

        if (relatorio.Status != StatusRelatorio.finalizado)
            throw new BusinessRuleException("Só um relatório finalizado pode ser reaberto.");

        relatorio.Status = StatusRelatorio.rascunho;
        relatorio.AtualizadoEm = Hoje;
        AdicionarHistorico(relatorio, userId, AcaoRelatorio.REABERTO, "", "", "");

        relatorios.Update(relatorio);
        await uow.SaveChangesAsync(ct);

        return await RecarregarAsync(id, ct);
    }

    public async Task<RelatorioDto> AprovarAsync(Guid userId, Role role, Guid id, CancellationToken ct = default)
    {
        if (role is not (Role.GESTOR or Role.ADMIN))
            throw new ForbiddenException("Só GESTOR e ADMIN podem aprovar relatórios.");

        var relatorio = await CarregarAsync(id, ct);

        if (relatorio.Status != StatusRelatorio.finalizado)
            throw new BusinessRuleException("Só um relatório finalizado pode ser aprovado.");

        relatorio.Status = StatusRelatorio.aprovado;
        relatorio.AtualizadoEm = Hoje;

        // Se ninguém assinou, a assinatura do responsável passa a ser o nome de quem aprovou.
        if (string.IsNullOrWhiteSpace(relatorio.AssinaturaResponsavel))
        {
            var aprovador = await users.GetByIdAsync(userId, ct);
            relatorio.AssinaturaResponsavel = aprovador?.Nome ?? "";
        }

        AdicionarHistorico(relatorio, userId, AcaoRelatorio.APROVADO, "", "", "");

        await notificacaoService.CriarAsync(
            relatorio.AutorId,
            "O seu relatório técnico foi aprovado.",
            $"/relatorios-tecnicos/{relatorio.Id}", ct);

        relatorios.Update(relatorio);
        await uow.SaveChangesAsync(ct);

        return await RecarregarAsync(id, ct);
    }

    // ─── Auxiliares ───────────────────────────────────────

    private async Task<RelatorioTecnico> CarregarAsync(Guid id, CancellationToken ct)
        => await relatorios.GetComHistoricoAsync(id, ct)
           ?? throw new NotFoundException("Relatório técnico não encontrado.");

    private async Task<RelatorioDto> RecarregarAsync(Guid id, CancellationToken ct)
        => RelatorioMapper.ToDto(await CarregarAsync(id, ct));

    private static void GarantirEscrita(RelatorioTecnico relatorio, Guid userId, Role role)
    {
        if (relatorio.AutorId != userId && role != Role.ADMIN)
            throw new ForbiddenException("Só o autor do relatório (ou um ADMIN) pode alterá-lo.");
    }

    private async Task GarantirLeituraAsync(Guid userId, Role role, RelatorioTecnico relatorio, CancellationToken ct)
    {
        if (role is Role.ADMIN or Role.TECNICO)
            return;

        if (role == Role.GESTOR)
        {
            if (relatorio.Status is not (StatusRelatorio.finalizado or StatusRelatorio.aprovado))
                throw new NotFoundException("Relatório técnico não encontrado.");
            return;
        }

        // FUNCIONARIO: só relatórios aprovados das suas próprias solicitações.
        // Sem acesso devolve 404 (e não 403) para não revelar a existência do relatório.
        if (relatorio.Status != StatusRelatorio.aprovado || relatorio.Ordem is null)
            throw new NotFoundException("Relatório técnico não encontrado.");

        var solicitacao = await solicitacoes.GetByIdAsync(relatorio.Ordem.SolicitacaoId, ct);
        if (solicitacao is null || solicitacao.SolicitanteId != userId)
            throw new NotFoundException("Relatório técnico não encontrado.");
    }

    private static void AdicionarHistorico(RelatorioTecnico relatorio, Guid autorId, AcaoRelatorio acao, string campo, string valorAntigo, string valorNovo)
        => relatorio.Historico.Add(new RelatorioHistorico
        {
            RelatorioId = relatorio.Id,
            Data = Hoje,
            Posicao = relatorio.Historico.Count,
            AutorId = autorId,
            Acao = acao,
            Campo = campo,
            ValorAntigo = valorAntigo,
            ValorNovo = valorNovo,
        });

    /// <summary>Serializa a lista de peças em JSON para o histórico (a lista inteira, não só a diferença).</summary>
    private static string SerializarPecas(List<PecaRelatorio> pecas)
        => JsonSerializer.Serialize(pecas.Select(p => new PecaRelatorioDto(p.Nome, p.Quantidade, p.Codigo)));
}
