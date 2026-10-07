using Novati.API.Common.Exceptions;
using Novati.API.Dtos.Atendimentos;
using Novati.API.Dtos.Compras;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>
/// Ciclo de vida da ordem de reparo. Cada método só avança o estado se a ordem estiver
/// no estado de partida esperado — é a máquina de estados do plano, escrita em código.
/// </summary>
public class AtendimentoService(
    IOrdemRepository ordens,
    ISolicitacaoRepository solicitacoes,
    IUserRepository users,
    IItemStockRepository itens,
    IUnidadeStockRepository unidades,
    IMovimentoStockRepository movimentos,
    ICompraRepository compras,
    IInstanciaRepository instancias,
    IDispositivoRepository dispositivos,
    ICompatibilidadeRepository compatibilidades,
    INotificacaoService notificacaoService,
    IAcessoRemotoService acessoRemotoService,
    IUnitOfWork uow) : IAtendimentoService
{
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);

    // ─── Leitura ──────────────────────────────────────────

    public async Task<List<OrdemDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default)
    {
        // Regra de ouro nº 4: o GET filtra, nunca devolve 403.
        IEnumerable<Guid>? filtro = null;
        if (role == Role.FUNCIONARIO)
        {
            var minhas = await solicitacoes.GetBySolicitanteAsync(userId, ct);
            filtro = [.. minhas.Select(s => s.Id)];
        }

        return (await ordens.GetTodasCompletasAsync(filtro, ct)).Select(OrdemMapper.ToDto).ToList();
    }

    // ─── Fluxo linear ─────────────────────────────────────

    public async Task<OrdemDto> AssumirAsync(Guid tecnicoId, Guid solicitacaoId, CancellationToken ct = default)
    {
        var solicitacao = await solicitacoes.GetByIdAsync(solicitacaoId, ct)
                         ?? throw new NotFoundException("Solicitação não encontrada.");

        if (solicitacao.Estado != EstadoSolicitacao.ABERTA)
            throw new ConflictException("Esta solicitação já foi assumida.");

        var tecnico = await users.GetByIdAsync(tecnicoId, ct)
                      ?? throw new NotFoundException("Técnico não encontrado.");

        var ordem = new OrdemReparo
        {
            SolicitacaoId = solicitacao.Id,
            TecnicoId = tecnicoId,
            Estado = EstadoOrdem.EM_DIAGNOSTICO,
            DataInicio = Hoje,
            IniciadaEm = DateTime.UtcNow,   // arranque do cronómetro
            Diagnostico = "",
        };

        await ordens.AddAsync(ordem, ct);

        solicitacao.Estado = EstadoSolicitacao.EM_ATENDIMENTO;
        solicitacoes.Update(solicitacao);

        AdicionarHistorico(ordem, TipoHistoricoOrdem.ASSUMIDA, $"Solicitação assumida por {tecnico.Nome}.", tecnicoId);

        await notificacaoService.CriarAsync(
            solicitacao.SolicitanteId,
            $"A sua solicitação \"{solicitacao.Titulo}\" foi assumida por {tecnico.Nome}.",
            "/solicitacoes", ct);

        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordem.Id, ct);
    }

    public async Task<OrdemDto> GuardarDiagnosticoAsync(Guid userId, Role role, Guid ordemId, DiagnosticoRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarETerDonoAsync(ordemId, userId, role, ct);

        if (ordem.Estado != EstadoOrdem.EM_DIAGNOSTICO)
            throw new BusinessRuleException("O diagnóstico só pode ser registado em EM_DIAGNOSTICO.");

        ordem.Diagnostico = request.Diagnostico;
        ordens.Update(ordem);
        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    public async Task<OrdemDto> ConcluirDiagnosticoAsync(Guid userId, Role role, Guid ordemId, CancellationToken ct = default)
    {
        var ordem = await CarregarETerDonoAsync(ordemId, userId, role, ct);

        if (ordem.Estado != EstadoOrdem.EM_DIAGNOSTICO)
            throw new BusinessRuleException("Só se pode concluir o diagnóstico a partir de EM_DIAGNOSTICO.");

        if (string.IsNullOrWhiteSpace(ordem.Diagnostico))
            throw new BusinessRuleException("Preencha o diagnóstico antes de avançar.");

        ordem.Estado = EstadoOrdem.EM_REPARACAO;
        AdicionarHistorico(ordem, TipoHistoricoOrdem.DIAGNOSTICO, ordem.Diagnostico, userId);

        ordens.Update(ordem);
        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    public async Task<OrdemDto> AdicionarComentarioAsync(Guid userId, Role role, Guid ordemId, ComentarioRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarAsync(ordemId, ct);

        var podeComentar = ordem.TecnicoId == userId || role is Role.ADMIN or Role.GESTOR;
        if (!podeComentar)
            throw new ForbiddenException("Sem permissão para comentar nesta ordem.");

        AdicionarHistorico(ordem, TipoHistoricoOrdem.COMENTARIO, request.Texto, userId);

        ordens.Update(ordem);
        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    public async Task<OrdemDto> ReatribuirAsync(Guid userId, Role role, Guid ordemId, ReatribuirRequest request, CancellationToken ct = default)
    {
        if (role is not (Role.ADMIN or Role.GESTOR))
            throw new ForbiddenException("Só ADMIN e GESTOR podem reatribuir ordens.");

        var ordem = await CarregarAsync(ordemId, ct);

        var novoTecnico = await users.GetByIdAsync(request.NovoTecnicoId, ct)
                          ?? throw new NotFoundException("Técnico não encontrado.");

        if (novoTecnico.Role != Role.TECNICO)
            throw new BusinessRuleException("A ordem só pode ser atribuída a um técnico.");

        ordem.TecnicoId = novoTecnico.Id;
        AdicionarHistorico(ordem, TipoHistoricoOrdem.REATRIBUIDA, $"Ordem reatribuída a {novoTecnico.Nome}.", userId);

        ordens.Update(ordem);

        // O acesso remoto foi consentido ao técnico anterior: não passa para o novo.
        await acessoRemotoService.EncerrarEmCursoAsync(ordem.SolicitacaoId, "A ordem foi reatribuída a outro técnico.", userId, ct);

        await notificacaoService.CriarAsync(
            novoTecnico.Id,
            $"Tens uma ordem de reparo reatribuída: {ordem.Solicitacao?.Titulo}.",
            "/atendimentos", ct);

        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    // ─── Algoritmo A: reservar peça ───────────────────────

    public async Task<ReservaResultado> ReservarPecaAsync(Guid userId, Role role, Guid ordemId, ReservarPecaRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarETerDonoAsync(ordemId, userId, role, ct);

        if (ordem.Estado != EstadoOrdem.EM_REPARACAO)
            throw new BusinessRuleException("Só se podem reservar peças com a ordem em EM_REPARACAO.");

        // Mesmo método partilhado com o StockService (Módulo 9) — não reimplementado aqui.
        var item = await itens.GetOrCreateByModeloAsync(request.ModeloComponenteId, ct);
        var unidade = await unidades.GetDisponivelAsync(item.Id, ct);

        if (unidade is not null)
        {
            unidade.Estado = EstadoUnidade.RESERVADA;
            unidade.ReservadaParaOrdemId = ordem.Id;
            unidades.Update(unidade);

            var peca = new OrdemPecaUsada
            {
                OrdemId = ordem.Id,
                UnidadeStockId = unidade.Id,
                ModeloComponenteId = request.ModeloComponenteId,
                InstaladoInstanciaId = null,
            };
            await ordens.AddPecaAsync(peca, ct);

            await movimentos.AddAsync(new MovimentoStock
            {
                ItemStockId = item.Id,
                Tipo = TipoMovimento.RESERVA,
                Quantidade = 1,
                Data = Hoje,
                Observacao = $"Reservado para a ordem {ordem.Id}.",
            }, ct);

            await uow.SaveChangesAsync(ct);

            var dto = await MapearAsync(ordemId, ct);
            return new ReservaResultado(true, dto, unidade.Id, unidade.Codigo, null);
        }

        // Sem stock: dispara o fluxo de compras (ICompraRepository, Módulo 11).
        var requisicao = new RequisicaoCompra
        {
            ItemStockId = item.Id,
            ModeloComponenteId = request.ModeloComponenteId,
            Quantidade = 1,
            Justificativa = $"Sem stock para a ordem {ordem.Id}.",
            SolicitanteId = userId,
            OrdemId = ordem.Id,
            Data = Hoje,
            Estado = EstadoRequisicao.PENDENTE,
        };
        await compras.AddAsync(requisicao, ct);

        await notificacaoService.NotificarPerfisAsync(
            [Role.GESTOR, Role.ADMIN],
            $"Requisição de compra: {request.ModeloComponenteId} para a ordem {ordem.Id}.",
            "/compras", ct);

        await uow.SaveChangesAsync(ct);

        var dtoSemStock = await MapearAsync(ordemId, ct);
        return new ReservaResultado(false, dtoSemStock, Guid.Empty, "", RequisicaoMapper.ToDto(requisicao));
    }

    // ─── Algoritmo B: instalar peça ───────────────────────

    public async Task<OrdemDto> InstalarPecaAsync(Guid userId, Role role, Guid ordemId, InstalarPecaRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarETerDonoAsync(ordemId, userId, role, ct);

        var unidade = await unidades.GetByIdAsync(request.UnidadeStockId, ct)
                      ?? throw new NotFoundException("Unidade de stock não encontrada.");

        if (unidade.Estado != EstadoUnidade.RESERVADA || unidade.ReservadaParaOrdemId != ordem.Id)
            throw new BusinessRuleException("Esta unidade não está reservada para esta ordem.");

        var peca = ordem.PecasUsadas.FirstOrDefault(p => p.UnidadeStockId == unidade.Id)
                   ?? throw new BusinessRuleException("Esta peça não foi reservada nesta ordem.");

        var dispositivo = await dispositivos.GetByIdAsync(request.DispositivoFisicoId, ct)
                          ?? throw new NotFoundException("Dispositivo não encontrado.");

        await ValidarCompatibilidadeAsync(dispositivo.ModeloDispositivoId, peca.ModeloComponenteId, ct);

        var novaInstancia = new InstanciaComponente
        {
            Codigo = unidade.Codigo,
            Estado = EstadoInstancia.INSTALADO,
            DispositivoFisicoId = dispositivo.Id,
            ModeloComponenteId = peca.ModeloComponenteId,
        };
        await instancias.AddAsync(novaInstancia, ct);

        unidade.Estado = EstadoUnidade.INSTALADA;
        unidades.Update(unidade);

        peca.InstaladoInstanciaId = novaInstancia.Id;

        if (request.InstanciaAntigaId is not null)
        {
            var antiga = await instancias.GetByIdAsync(request.InstanciaAntigaId.Value, ct)
                         ?? throw new NotFoundException("Instância anterior não encontrada.");

            if (antiga.DispositivoFisicoId != dispositivo.Id)
                throw new BusinessRuleException("A instância anterior pertence a outro dispositivo.");

            instancias.Remove(antiga);
        }

        await movimentos.AddAsync(new MovimentoStock
        {
            ItemStockId = unidade.ItemStockId,
            Tipo = TipoMovimento.INSTALACAO,
            Quantidade = 1,
            Data = Hoje,
            Observacao = $"Instalado no dispositivo {dispositivo.Patrimonio}.",
        }, ct);

        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    // ─── Solução ──────────────────────────────────────────

    public async Task<OrdemDto> ProporSolucaoAsync(Guid userId, Role role, Guid ordemId, PropostaSolucaoRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarETerDonoAsync(ordemId, userId, role, ct);

        if (ordem.Estado is not (EstadoOrdem.EM_REPARACAO or EstadoOrdem.EM_DIAGNOSTICO))
            throw new BusinessRuleException("Só se pode propor solução a partir de EM_DIAGNOSTICO ou EM_REPARACAO.");

        ordem.Estado = EstadoOrdem.AGUARDA_VALIDACAO;
        ordem.Solucao = request.Solucao;
        // A partir daqui a ordem está nas mãos do solicitante: marca o início da espera.
        ordem.PropostaEm = DateTime.UtcNow;
        AdicionarHistorico(ordem, TipoHistoricoOrdem.SOLUCAO, request.Solucao, userId);

        var solicitacao = ordem.Solicitacao;
        if (solicitacao is not null)
        {
            solicitacao.Estado = EstadoSolicitacao.AGUARDA_VALIDACAO;
            solicitacoes.Update(solicitacao);
        }

        ordens.Update(ordem);

        if (solicitacao is not null)
            await notificacaoService.CriarAsync(
                solicitacao.SolicitanteId,
                $"A solução para \"{solicitacao.Titulo}\" está à tua espera para validação.",
                "/solicitacoes", ct);

        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    // ─── Algoritmo C: responder validação ─────────────────

    public async Task<OrdemDto> ResponderValidacaoAsync(Guid userId, Guid ordemId, ResponderValidacaoRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarAsync(ordemId, ct);

        var solicitacao = ordem.Solicitacao
                          ?? throw new NotFoundException("Solicitação da ordem não encontrada.");

        // Só o solicitante da própria solicitação valida (não basta ser FUNCIONARIO).
        if (solicitacao.SolicitanteId != userId)
            throw new ForbiddenException("Só o solicitante pode validar a solução.");

        if (ordem.Estado != EstadoOrdem.AGUARDA_VALIDACAO)
            throw new BusinessRuleException("Esta ordem não está à espera de validação.");

        if (!request.Aceite && string.IsNullOrWhiteSpace(request.Motivo))
            throw new BusinessRuleException("Indica o motivo da recusa.");

        // O tempo que o solicitante demorou a responder não é trabalho do técnico: soma-se à parte.
        var agora = DateTime.UtcNow;
        if (ordem.PropostaEm is { } propostaEm)
            ordem.EsperaValidacaoSeg += (int)(agora - propostaEm).TotalSeconds;
        ordem.PropostaEm = null;

        if (request.Aceite)
        {
            ordem.Estado = EstadoOrdem.RESOLVIDO;
            ordem.DataFim = Hoje;

            // Paragem do cronómetro: os tempos saem das horas gravadas pelo servidor.
            ordem.ConcluidaEm = agora;
            var total = agora - ordem.IniciadaEm;
            ordem.TempoGastoMin = EmMinutos(total);
            ordem.TempoTrabalhoMin = EmMinutos(total - TimeSpan.FromSeconds(ordem.EsperaValidacaoSeg));

            solicitacao.Estado = EstadoSolicitacao.RESOLVIDA;
            AdicionarHistorico(ordem, TipoHistoricoOrdem.ACEITE, "Solução validada pelo solicitante.", userId);

            // Atendimento concluído: não fica nenhuma sessão remota aberta.
            await acessoRemotoService.EncerrarEmCursoAsync(solicitacao.Id, "A solicitação foi resolvida.", userId, ct);

            await notificacaoService.CriarAsync(
                ordem.TecnicoId,
                $"\"{solicitacao.Titulo}\" foi validada e está resolvida.",
                "/atendimentos", ct);
        }
        else
        {
            var rejeicao = new OrdemRejeicao { OrdemId = ordem.Id, Motivo = request.Motivo, Data = Hoje };
            await ordens.AddRejeicaoAsync(rejeicao, ct);

            ordem.Estado = EstadoOrdem.EM_DIAGNOSTICO;
            solicitacao.Estado = EstadoSolicitacao.EM_ATENDIMENTO;
            AdicionarHistorico(ordem, TipoHistoricoOrdem.REJEITADA, request.Motivo, userId);

            await notificacaoService.CriarAsync(
                ordem.TecnicoId,
                $"A solução para \"{solicitacao.Titulo}\" foi recusada: {request.Motivo}",
                "/atendimentos", ct);
        }

        ordens.Update(ordem);
        solicitacoes.Update(solicitacao);
        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    public async Task<OrdemDto> GuardarSolucaoSugeridaAsync(Guid userId, Role role, Guid ordemId, SolucaoSugeridaRequest request, CancellationToken ct = default)
    {
        var ordem = await CarregarETerDonoAsync(ordemId, userId, role, ct);

        ordem.SolucaoSugerida = request.Solucao;
        ordens.Update(ordem);
        await uow.SaveChangesAsync(ct);

        return await MapearAsync(ordemId, ct);
    }

    // ─── Auxiliares ───────────────────────────────────────

    /// <summary>Compatibilidade validada "se possível": só bloqueia se o catálogo restringir o componente.</summary>
    private async Task ValidarCompatibilidadeAsync(Guid modeloDispositivoId, Guid modeloComponenteId, CancellationToken ct)
    {
        if (await compatibilidades.ExisteAlgumaAsync(modeloComponenteId, ct)
            && !await compatibilidades.ExistsPairAsync(modeloDispositivoId, modeloComponenteId, ct))
            throw new BusinessRuleException("Este componente não é compatível com o modelo do dispositivo.");
    }

    /// <summary>Duração em minutos inteiros, arredondada para cima (nunca menos de 1).</summary>
    private static int EmMinutos(TimeSpan duracao)
        => Math.Max(1, (int)Math.Ceiling(duracao.TotalMinutes));

    private async Task<OrdemReparo> CarregarAsync(Guid ordemId, CancellationToken ct)
        => await ordens.GetCompletaAsync(ordemId, ct)
           ?? throw new NotFoundException("Ordem não encontrada.");

    /// <summary>Carrega a ordem e confirma que quem chama é o técnico dono ou um ADMIN.</summary>
    private async Task<OrdemReparo> CarregarETerDonoAsync(Guid ordemId, Guid userId, Role role, CancellationToken ct)
    {
        var ordem = await CarregarAsync(ordemId, ct);

        if (ordem.TecnicoId != userId && role != Role.ADMIN)
            throw new ForbiddenException("Só o técnico responsável (ou um ADMIN) pode alterar esta ordem.");

        return ordem;
    }

    private static void AdicionarHistorico(OrdemReparo ordem, TipoHistoricoOrdem tipo, string texto, Guid autorId)
        => ordem.Historico.Add(new OrdemHistorico
        {
            OrdemId = ordem.Id,
            Tipo = tipo,
            Texto = texto,
            Data = Hoje,
            Posicao = ordem.Historico.Count,
            AutorId = autorId,
        });

    /// <summary>Recarrega a ordem completa para garantir que o DTO inclui todas as coleções aninhadas.</summary>
    private async Task<OrdemDto> MapearAsync(Guid ordemId, CancellationToken ct)
    {
        var completa = await ordens.GetCompletaAsync(ordemId, ct)
                       ?? throw new NotFoundException("Ordem não encontrada.");
        return OrdemMapper.ToDto(completa);
    }
}
