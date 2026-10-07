using Novati.API.Dtos.AcessoRemoto;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IAcessoRemotoService
{
    Task<List<SessaoRemotaDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default);

    /// <summary>O técnico da ordem pede acesso ao PC do solicitante.</summary>
    Task<SessaoRemotaDto> PedirAsync(Guid tecnicoId, Guid solicitacaoId, PedirAcessoRequest? request, CancellationToken ct = default);

    /// <summary>O solicitante autoriza: gera o token de uso único e entrega-o ao técnico.</summary>
    Task<AutorizacaoResponse> AutorizarAsync(Guid userId, Guid sessaoId, AutorizarRequest? request, string? ip, CancellationToken ct = default);

    Task<SessaoRemotaDto> RecusarAsync(Guid userId, Guid sessaoId, MotivoRequest? request, CancellationToken ct = default);

    /// <summary>O técnico troca o token pela sessão ativa. Só funciona uma vez.</summary>
    Task<ResgateResponse> ResgatarAsync(Guid userId, Guid sessaoId, ResgatarRequest request, CancellationToken ct = default);

    /// <summary>Termina uma sessão ativa ou cancela um pedido que ainda não começou.</summary>
    Task<SessaoRemotaDto> TerminarAsync(Guid userId, Role role, Guid sessaoId, MotivoRequest? request, CancellationToken ct = default);

    /// <summary>Fim pedido do lado do PC do solicitante (botão do agente, ou agente fechado).</summary>
    Task TerminarPeloAgenteAsync(Guid sessaoId, string motivo, CancellationToken ct = default);

    Task<IceResponse> GetIceAsync(Guid userId, Guid sessaoId, CancellationToken ct = default);

    /// <summary>
    /// Encerra a sessão em curso de uma solicitação (se houver). Só altera o contexto —
    /// quem chama grava tudo numa só transação (SaveChanges próprio).
    /// </summary>
    Task EncerrarEmCursoAsync(Guid solicitacaoId, string motivo, Guid? autorId, CancellationToken ct = default);

    /// <summary>Fecha as sessões que passaram do prazo. Devolve quantas fechou.</summary>
    Task<int> ExpirarVencidasAsync(CancellationToken ct = default);
}
