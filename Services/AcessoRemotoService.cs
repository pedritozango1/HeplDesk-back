using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Settings;
using Novati.API.Dtos.AcessoRemoto;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Realtime;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;
using Npgsql;

namespace Novati.API.Services;

/// <summary>
/// Acesso remoto ao PC do solicitante: pedido do técnico, consentimento do solicitante,
/// token de uso único e ciclo de vida da sessão. Cada método só avança o estado se a
/// sessão estiver no estado de partida esperado.
/// </summary>
public class AcessoRemotoService(
    ISessaoRemotaRepository sessoes,
    ISolicitacaoRepository solicitacoes,
    IOrdemRepository ordens,
    IUserRepository users,
    INotificacaoService notificacaoService,
    ServidoresIceService servidoresIce,
    IOptions<AcessoRemotoSettings> opcoes,
    IHubContext<TempoRealHub> hub,
    AgentesLigados agentes,
    IHubContext<AgenteHub> hubAgentes,
    IUnitOfWork uow,
    ILogger<AcessoRemotoService> logger) : IAcessoRemotoService
{
    private AcessoRemotoSettings Cfg => opcoes.Value;

    // ─── Leitura ──────────────────────────────────────────

    public async Task<List<SessaoRemotaDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default)
        => (await sessoes.GetVisiveisAsync(userId, role, ct)).Select(SessaoRemotaMapper.ToDto).ToList();

    public async Task<IceResponse> GetIceAsync(Guid userId, Guid sessaoId, CancellationToken ct = default)
    {
        var sessao = await CarregarAsync(sessaoId, ct);

        if (!Participa(sessao, userId))
            throw new ForbiddenException("Não participa nesta sessão de acesso remoto.");

        if (sessao.Estado != EstadoSessaoRemota.ATIVA)
            throw new ConflictException("Esta sessão não está ativa.", "ESTADO_INVALIDO");

        var ice = sessao.Modo == ModoAcessoRemoto.VER ? servidoresIce.Obter(userId) : [];
        return new IceResponse(ice, LimiteDe(sessao));
    }

    // ─── Pedido ───────────────────────────────────────────

    public async Task<SessaoRemotaDto> PedirAsync(Guid tecnicoId, Guid solicitacaoId, PedirAcessoRequest? request, CancellationToken ct = default)
    {
        var solicitacao = await solicitacoes.GetByIdAsync(solicitacaoId, ct)
                          ?? throw new NotFoundException("Solicitação não encontrada.");

        // Só quem assumiu a solicitação pede acesso — não basta ser técnico.
        if (await ordens.GetTecnicoIdPorSolicitacaoAsync(solicitacaoId, ct) != tecnicoId)
            throw new ForbiddenException("Só o técnico que assumiu a solicitação pode pedir acesso remoto.");

        if (solicitacao.Estado is not (EstadoSolicitacao.EM_ATENDIMENTO or EstadoSolicitacao.AGUARDA_VALIDACAO))
            throw new BusinessRuleException("Só se pode pedir acesso remoto a uma solicitação em atendimento.", "ESTADO_INVALIDO");

        if (!Enum.TryParse<ModoAcessoRemoto>(request?.Modo ?? nameof(ModoAcessoRemoto.VER), out var modo))
            throw new BusinessRuleException("Modo de acesso inválido.", "MODO_INVALIDO");

        var emCurso = await sessoes.GetEmCursoPorSolicitacaoAsync(solicitacaoId, ct);
        if (emCurso is not null)
        {
            if (!await CaducarAsync(emCurso, DateTime.UtcNow, ct))
                throw new ConflictException("Já existe um pedido de acesso remoto em curso para esta solicitação.", "SESSAO_EM_CURSO");

            // Estava vencida: fecha-se primeiro, para o índice único deixar entrar a nova.
            await GravarAsync(ct);
        }

        var tecnico = await users.GetByIdAsync(tecnicoId, ct)
                      ?? throw new NotFoundException("Técnico não encontrado.");

        var sessao = new SessaoRemota
        {
            SolicitacaoId = solicitacao.Id,
            TecnicoId = tecnicoId,
            SolicitanteId = solicitacao.SolicitanteId,
            Estado = EstadoSessaoRemota.PEDIDA,
            Modo = modo,
            PedidaEm = DateTime.UtcNow,
        };
        await sessoes.AddAsync(sessao, ct);

        await notificacaoService.CriarAsync(
            solicitacao.SolicitanteId,
            modo == ModoAcessoRemoto.CONTROLAR
                ? $"{tecnico.Nome} pede para controlar o seu PC por causa de \"{solicitacao.Titulo}\"."
                : $"{tecnico.Nome} pede para ver o seu ecrã por causa de \"{solicitacao.Titulo}\".",
            "/solicitacoes", ct);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        // Dois pedidos ao mesmo tempo: ambos passaram a verificação acima, mas o índice
        // único parcial da BD só deixa gravar um.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("Já existe um pedido de acesso remoto em curso para esta solicitação.", "SESSAO_EM_CURSO");
        }

        return SessaoRemotaMapper.ToDto(sessao);
    }

    // ─── Resposta do solicitante ──────────────────────────

    public async Task<AutorizacaoResponse> AutorizarAsync(Guid userId, Guid sessaoId, AutorizarRequest? request, string? ip, CancellationToken ct = default)
    {
        var sessao = await CarregarParaResponderAsync(sessaoId, userId, ct);
        var agora = DateTime.UtcNow;

        if (sessao.Modo == ModoAcessoRemoto.CONTROLAR)
        {
            // O código que o Agente Novati mostra no ecrã do PC ("482 913 057"): prova que quem
            // está a autorizar, com sessão iniciada, está mesmo à frente desse PC.
            var codigo = new string((request?.CodigoAgente ?? "").Where(char.IsDigit).ToArray());
            if (codigo.Length == 0)
                throw new BusinessRuleException("Indique o código que o Agente Novati mostra no seu PC.", "AGENTE_CODIGO_EM_FALTA");

            var agente = agentes.Reservar(codigo, sessao.Id)
                         ?? throw new BusinessRuleException(
                             "Não há nenhum agente disponível com esse código. Confirme que o Agente Novati está aberto neste PC e copie o código que ele mostra.",
                             "AGENTE_NAO_ENCONTRADO");

            sessao.AgentePc = agente.NomePc;
        }

        // O token em claro só existe nesta variável: para a BD vai apenas o hash.
        var token = TokenAcesso.Gerar();
        sessao.TokenHash = TokenAcesso.Hash(sessao.Id, token);
        sessao.TokenExpiraEm = agora.AddSeconds(Cfg.TokenTtlSegundos);
        sessao.TentativasFalhadas = 0;
        sessao.Estado = EstadoSessaoRemota.AUTORIZADA;
        sessao.RespondidaEm = agora;
        sessao.AutorizadaIp = ip;
        sessoes.Update(sessao);

        // A notificação fica gravada e é visível mais tarde: nunca leva o código.
        await notificacaoService.CriarAsync(
            sessao.TecnicoId,
            "O acesso remoto foi autorizado. Introduza o código para começar.",
            "/atendimentos", ct);

        try
        {
            await GravarAsync(ct);
        }
        catch
        {
            // A autorização não ficou gravada: o agente não pode continuar reservado para ela.
            agentes.Libertar(sessao.Id);
            throw;
        }

        var formatado = TokenAcesso.Formatar(token);
        try
        {
            // Entrega dirigida: só as ligações deste técnico recebem o código (nunca Clients.All).
            await hub.Clients.Group(TempoRealHub.GrupoUser(sessao.TecnicoId)).SendAsync(
                "acessoRemotoToken",
                new { sessaoId = sessao.Id, token = formatado, expiraEm = sessao.TokenExpiraEm },
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            // O solicitante vê o código na resposta e pode ditá-lo: a entrega falhada não desfaz a autorização.
            logger.LogWarning(ex, "Falha ao entregar o código da sessão remota {SessaoId} ao técnico", sessao.Id);
        }

        return new AutorizacaoResponse(SessaoRemotaMapper.ToDto(sessao), formatado, sessao.TokenExpiraEm.Value);
    }

    public async Task<SessaoRemotaDto> RecusarAsync(Guid userId, Guid sessaoId, MotivoRequest? request, CancellationToken ct = default)
    {
        var sessao = await CarregarParaResponderAsync(sessaoId, userId, ct);
        var agora = DateTime.UtcNow;

        sessao.RespondidaEm = agora;
        await FecharAsync(sessao, EstadoSessaoRemota.RECUSADA, Texto(request?.Motivo) ?? "Recusado pelo solicitante.", userId, agora, ct);
        sessoes.Update(sessao);

        await notificacaoService.CriarAsync(
            sessao.TecnicoId,
            $"O pedido de acesso remoto foi recusado: {sessao.MotivoFim}",
            "/atendimentos", ct);

        await GravarAsync(ct);

        return SessaoRemotaMapper.ToDto(sessao);
    }

    // ─── Resgate do token ─────────────────────────────────

    public async Task<ResgateResponse> ResgatarAsync(Guid userId, Guid sessaoId, ResgatarRequest request, CancellationToken ct = default)
    {
        var sessao = await CarregarAsync(sessaoId, ct);
        var agora = DateTime.UtcNow;

        // 1) Permissão: o código só serve ao técnico que pediu — e só enquanto a ordem for dele.
        if (sessao.TecnicoId != userId
            || await ordens.GetTecnicoIdPorSolicitacaoAsync(sessao.SolicitacaoId, ct) != userId)
            throw new ForbiddenException("Só o técnico que pediu o acesso pode usar este código.");

        // 2) Estado.
        switch (sessao.Estado)
        {
            case EstadoSessaoRemota.AUTORIZADA:
                break;
            case EstadoSessaoRemota.PEDIDA:
                throw new ConflictException("O solicitante ainda não autorizou o acesso.", "ESTADO_INVALIDO");
            case EstadoSessaoRemota.ATIVA:
                throw new ConflictException("Este código já foi utilizado.", "TOKEN_USADO");
            case EstadoSessaoRemota.EXPIRADA:
                throw new ExpiradoException("O código expirou. Peça um novo acesso.", "TOKEN_EXPIRADO");
            default:
                throw new ConflictException("Esta sessão já terminou. Peça um novo acesso.", "ESTADO_INVALIDO");
        }

        // 3) Prazo.
        if (await CaducarAsync(sessao, agora, ct))
        {
            await GravarAsync(ct);
            throw new ExpiradoException("O código expirou. Peça um novo acesso.", "TOKEN_EXPIRADO");
        }

        // 4) Formato — um código mal escrito não gasta tentativa.
        var token = TokenAcesso.Normalizar(request.Token);
        if (token.Length != TokenAcesso.Tamanho)
            throw new BusinessRuleException($"O código tem {TokenAcesso.Tamanho} caracteres.", "TOKEN_FORMATO");

        // 5) O código em si.
        if (!TokenAcesso.Verificar(sessao.Id, token, sessao.TokenHash))
        {
            sessao.TentativasFalhadas++;
            var restam = Cfg.MaxTentativas - sessao.TentativasFalhadas;
            if (restam <= 0)
                await FecharAsync(sessao, EstadoSessaoRemota.EXPIRADA, "Demasiados códigos errados.", null, agora, ct);
            sessoes.Update(sessao);

            // Grava ANTES de lançar: a exceção interrompe o pedido e o contador perdia-se.
            await GravarAsync(ct);

            if (restam <= 0)
                throw new DemasiadasTentativasException("Demasiadas tentativas. Peça um novo acesso.", "TENTATIVAS_ESGOTADAS");

            throw new BusinessRuleException(
                restam == 1 ? "Código incorreto. Resta 1 tentativa." : $"Código incorreto. Restam {restam} tentativas.",
                "TOKEN_INVALIDO");
        }

        // No modo CONTROLAR o agente tem de continuar ligado: sem ele não há nada para controlar.
        var controlar = sessao.Modo == ModoAcessoRemoto.CONTROLAR;
        if (controlar && agentes.PorSessao(sessao.Id) is null)
        {
            await FecharAsync(sessao, EstadoSessaoRemota.EXPIRADA, "O agente desligou-se antes de a sessão começar.", null, agora, ct);
            sessoes.Update(sessao);
            await GravarAsync(ct);
            throw new ExpiradoException("O agente no PC do funcionário já não está ligado. Peça um novo acesso.", "AGENTE_DESLIGADO");
        }

        // Certo: consome-se o token e ativa-se a sessão na mesma gravação.
        sessao.TokenUsadoEm = agora;
        sessao.TokenHash = null;
        sessao.Estado = EstadoSessaoRemota.ATIVA;
        sessao.IniciadaEm = agora;
        sessoes.Update(sessao);

        await RegistarNoHistoricoAsync(
            sessao,
            controlar
                ? $"Acesso remoto iniciado (controlo do PC {sessao.AgentePc}), autorizado pelo solicitante."
                : "Acesso remoto iniciado (ver ecrã), autorizado pelo solicitante.",
            userId, ct);

        // Se dois resgates chegarem juntos com o código certo, o xmin só deixa passar um.
        await GravarAsync(ct, "Este código já foi utilizado.", "TOKEN_USADO");

        if (!controlar)
            return new ResgateResponse(SessaoRemotaMapper.ToDto(sessao), servidoresIce.Obter(userId), LimiteDe(sessao));

        // Só agora, com a sessão ATIVA já gravada, é que o agente é mandado começar.
        // O modo CONTROLAR não usa WebRTC: imagem e entrada passam pelo servidor.
        if (agentes.Ativar(sessao.Id) is { } agente)
        {
            var tecnico = await users.GetByIdAsync(userId, ct);
            await hubAgentes.Clients.Client(agente.LigacaoId)
                .SendAsync("iniciar", tecnico?.Nome ?? "O técnico", CancellationToken.None);
        }

        return new ResgateResponse(SessaoRemotaMapper.ToDto(sessao), [], LimiteDe(sessao));
    }

    /// <summary>
    /// Fim pedido do lado do PC: botão "Terminar" do agente, ou o agente fechou/perdeu a ligação.
    /// Não passa por permissões de utilizador — quem chama (AgenteHub) já confirmou que a ligação
    /// é a do agente associado a esta sessão.
    /// </summary>
    public async Task TerminarPeloAgenteAsync(Guid sessaoId, string motivo, CancellationToken ct = default)
    {
        var sessao = await sessoes.GetByIdAsync(sessaoId, ct);
        if (sessao is null || !sessao.EmCurso)
            return;

        var ativa = sessao.Estado == EstadoSessaoRemota.ATIVA;
        await FecharAsync(sessao, ativa ? EstadoSessaoRemota.TERMINADA : EstadoSessaoRemota.CANCELADA,
            motivo, sessao.SolicitanteId, DateTime.UtcNow, ct);
        sessoes.Update(sessao);

        try { await GravarAsync(ct); }
        catch (ConflictException) { /* outra via terminou a sessão no mesmo instante: o resultado é o mesmo */ }
    }

    // ─── Fim ──────────────────────────────────────────────

    public async Task<SessaoRemotaDto> TerminarAsync(Guid userId, Role role, Guid sessaoId, MotivoRequest? request, CancellationToken ct = default)
    {
        var sessao = await CarregarAsync(sessaoId, ct);

        if (!Participa(sessao, userId) && role is not (Role.ADMIN or Role.GESTOR))
            throw new ForbiddenException("Sem permissão para terminar esta sessão.");

        if (!sessao.EmCurso)
            throw new ConflictException("Esta sessão já terminou.", "ESTADO_INVALIDO");

        var quem = userId == sessao.SolicitanteId ? "pelo solicitante"
                 : userId == sessao.TecnicoId ? "pelo técnico"
                 : "pela gestão";

        // Uma sessão que nunca chegou a começar não "termina": é cancelada.
        var ativa = sessao.Estado == EstadoSessaoRemota.ATIVA;
        await FecharAsync(
            sessao,
            ativa ? EstadoSessaoRemota.TERMINADA : EstadoSessaoRemota.CANCELADA,
            Texto(request?.Motivo) ?? (ativa ? $"Terminada {quem}." : $"Cancelada {quem}."),
            userId, DateTime.UtcNow, ct);
        sessoes.Update(sessao);

        await GravarAsync(ct);

        return SessaoRemotaMapper.ToDto(sessao);
    }

    public async Task EncerrarEmCursoAsync(Guid solicitacaoId, string motivo, Guid? autorId, CancellationToken ct = default)
    {
        var sessao = await sessoes.GetEmCursoPorSolicitacaoAsync(solicitacaoId, ct);
        if (sessao is null)
            return;

        var ativa = sessao.Estado == EstadoSessaoRemota.ATIVA;
        await FecharAsync(sessao, ativa ? EstadoSessaoRemota.TERMINADA : EstadoSessaoRemota.CANCELADA, motivo, autorId, DateTime.UtcNow, ct);
        sessoes.Update(sessao);
    }

    public async Task<int> ExpirarVencidasAsync(CancellationToken ct = default)
    {
        var agora = DateTime.UtcNow;
        var vencidas = await sessoes.GetVencidasAsync(
            agora.AddSeconds(-Cfg.PedidoTtlSegundos), agora, agora.AddMinutes(-Cfg.DuracaoMaxMinutos), ct);

        var fechadas = 0;
        foreach (var sessao in vencidas)
            if (await CaducarAsync(sessao, agora, ct))
                fechadas++;

        if (fechadas > 0)
            await uow.SaveChangesAsync(ct);

        return fechadas;
    }

    // ─── Auxiliares ───────────────────────────────────────

    private async Task<SessaoRemota> CarregarAsync(Guid sessaoId, CancellationToken ct)
        => await sessoes.GetByIdAsync(sessaoId, ct)
           ?? throw new NotFoundException("Sessão de acesso remoto não encontrada.");

    /// <summary>Carrega um pedido e confirma que quem chama é o solicitante e que ainda está por responder.</summary>
    private async Task<SessaoRemota> CarregarParaResponderAsync(Guid sessaoId, Guid userId, CancellationToken ct)
    {
        var sessao = await CarregarAsync(sessaoId, ct);

        // O consentimento é do dono do PC: nem um ADMIN autoriza por ele.
        if (sessao.SolicitanteId != userId)
            throw new ForbiddenException("Só o solicitante pode responder a este pedido.");

        if (sessao.Estado != EstadoSessaoRemota.PEDIDA)
            throw new ConflictException("Este pedido já foi respondido.", "ESTADO_INVALIDO");

        if (await CaducarAsync(sessao, DateTime.UtcNow, ct))
        {
            await GravarAsync(ct);
            throw new ExpiradoException("O pedido expirou. O técnico tem de pedir novamente.", "PEDIDO_EXPIRADO");
        }

        return sessao;
    }

    private static bool Participa(SessaoRemota sessao, Guid userId)
        => sessao.TecnicoId == userId || sessao.SolicitanteId == userId;

    /// <summary>Momento em que uma sessão ativa termina sozinha.</summary>
    private DateTime LimiteDe(SessaoRemota sessao)
        => (sessao.IniciadaEm ?? DateTime.UtcNow).AddMinutes(Cfg.DuracaoMaxMinutos);

    private static string? Texto(string? motivo)
        => string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

    /// <summary>
    /// Fecha a sessão se já passou do prazo do estado em que está. Devolve true se fechou.
    /// É chamado em cada operação (não só pelo temporizador), por isso as regras de prazo
    /// valem mesmo que o serviço de limpeza esteja atrasado.
    /// </summary>
    private async Task<bool> CaducarAsync(SessaoRemota sessao, DateTime agora, CancellationToken ct)
    {
        switch (sessao.Estado)
        {
            case EstadoSessaoRemota.PEDIDA when agora > sessao.PedidaEm.AddSeconds(Cfg.PedidoTtlSegundos):
                await FecharAsync(sessao, EstadoSessaoRemota.EXPIRADA, "Pedido sem resposta a tempo.", null, agora, ct);
                return true;

            case EstadoSessaoRemota.AUTORIZADA when agora > sessao.TokenExpiraEm:
                await FecharAsync(sessao, EstadoSessaoRemota.EXPIRADA, "O código expirou sem ser usado.", null, agora, ct);
                return true;

            case EstadoSessaoRemota.ATIVA when agora > LimiteDe(sessao):
                await FecharAsync(sessao, EstadoSessaoRemota.TERMINADA, "Tempo máximo atingido.", null, agora, ct);
                return true;

            default:
                return false;
        }
    }

    /// <summary>Leva a sessão a um estado final. Se estava ativa, o fim fica também no histórico da ordem.</summary>
    private async Task FecharAsync(SessaoRemota sessao, EstadoSessaoRemota estadoFinal, string motivo, Guid? autorId, DateTime agora, CancellationToken ct)
    {
        var estavaAtiva = sessao.Estado == EstadoSessaoRemota.ATIVA;

        sessao.Estado = estadoFinal;
        sessao.TerminadaEm = agora;
        sessao.TerminadaPorId = autorId;
        sessao.MotivoFim = motivo.Length > 200 ? motivo[..200] : motivo;
        sessao.TokenHash = null;   // um token de sessão fechada não serve a ninguém
        // O agente reservado para esta sessão é libertado e mandado parar pelo UnitOfWork,
        // depois de o fim estar gravado.

        if (estavaAtiva && sessao.IniciadaEm is { } inicio)
        {
            var minutos = Math.Max(1, (int)Math.Ceiling((agora - inicio).TotalMinutes));
            await RegistarNoHistoricoAsync(sessao, $"Acesso remoto terminado ao fim de {minutos} min. {sessao.MotivoFim}", autorId, ct);
        }
    }

    private async Task RegistarNoHistoricoAsync(SessaoRemota sessao, string texto, Guid? autorId, CancellationToken ct)
    {
        var ordem = await ordens.GetComHistoricoPorSolicitacaoAsync(sessao.SolicitacaoId, ct);
        if (ordem is null)
            return;

        ordem.Historico.Add(new OrdemHistorico
        {
            OrdemId = ordem.Id,
            Tipo = TipoHistoricoOrdem.ACESSO_REMOTO,
            Texto = texto,
            Data = DateOnly.FromDateTime(DateTime.UtcNow),
            Posicao = ordem.Historico.Count,
            AutorId = autorId,
        });
    }

    /// <summary>
    /// SaveChanges com a concorrência traduzida: se outro pedido gravou a mesma sessão
    /// entretanto (xmin diferente), é um 409 e não um erro interno.
    /// </summary>
    private async Task GravarAsync(
        CancellationToken ct,
        string mensagemConflito = "Este pedido foi alterado entretanto. Atualize e tente de novo.",
        string codigoConflito = "CONFLITO")
    {
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(mensagemConflito, codigoConflito);
        }
    }
}
