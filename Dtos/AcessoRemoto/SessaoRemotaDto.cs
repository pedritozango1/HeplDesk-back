namespace Novati.API.Dtos.AcessoRemoto;

/// <summary>Pedido/sessão de acesso remoto. Nunca inclui o token nem o seu hash.</summary>
public record SessaoRemotaDto(
    Guid Id,
    Guid SolicitacaoId,
    Guid TecnicoId,
    Guid SolicitanteId,
    string Estado,
    string Modo,
    DateTime PedidaEm,
    DateTime? RespondidaEm,
    DateTime? TokenExpiraEm,
    DateTime? IniciadaEm,
    DateTime? TerminadaEm,
    Guid? TerminadaPorId,
    string? MotivoFim,
    string? AgentePc);

/// <summary>Resposta à autorização: a única vez que o token sai da API por REST.</summary>
public record AutorizacaoResponse(SessaoRemotaDto Sessao, string Token, DateTime ExpiraEm);

/// <summary>Servidor ICE no formato que o RTCPeerConnection do browser aceita.</summary>
public record IceServerDto(string[] Urls, string? Username = null, string? Credential = null);

/// <summary>
/// Resposta ao resgate do token: a sessão já ATIVA. No modo VER traz os servidores ICE para o
/// WebRTC; no modo CONTROLAR a lista vem vazia (imagem e entrada passam pelo servidor).
/// </summary>
public record ResgateResponse(SessaoRemotaDto Sessao, List<IceServerDto> IceServers, DateTime ExpiraEm);

/// <summary>Servidores ICE de uma sessão ativa e o momento em que ela termina sozinha.</summary>
public record IceResponse(List<IceServerDto> IceServers, DateTime ExpiraEm);
