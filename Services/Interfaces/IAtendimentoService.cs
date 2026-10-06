using Novati.API.Dtos.Atendimentos;
using Novati.API.Dtos.Compras;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IAtendimentoService
{
    /// <summary>ADMIN/GESTOR/TECNICO veem todas as ordens; FUNCIONARIO só as das suas solicitações.</summary>
    Task<List<OrdemDto>> GetVisiveisAsync(Guid userId, Role role, CancellationToken ct = default);

    Task<OrdemDto> AssumirAsync(Guid tecnicoId, Guid solicitacaoId, CancellationToken ct = default);
    Task<OrdemDto> GuardarDiagnosticoAsync(Guid userId, Role role, Guid ordemId, DiagnosticoRequest request, CancellationToken ct = default);
    Task<OrdemDto> ConcluirDiagnosticoAsync(Guid userId, Role role, Guid ordemId, CancellationToken ct = default);
    Task<OrdemDto> AdicionarComentarioAsync(Guid userId, Role role, Guid ordemId, ComentarioRequest request, CancellationToken ct = default);
    Task<OrdemDto> ReatribuirAsync(Guid userId, Role role, Guid ordemId, ReatribuirRequest request, CancellationToken ct = default);

    /// <summary>
    /// Algoritmo A: reserva a peça ou, em falta de stock, cria a requisição de compra.
    /// O controller converte este resultado na <c>ReservaOkResponse</c> ou <c>ReservaFalhaResponse</c>.
    /// </summary>
    Task<ReservaResultado> ReservarPecaAsync(Guid userId, Role role, Guid ordemId, ReservarPecaRequest request, CancellationToken ct = default);

    /// <summary>Algoritmo B: instala a unidade reservada no dispositivo físico.</summary>
    Task<OrdemDto> InstalarPecaAsync(Guid userId, Role role, Guid ordemId, InstalarPecaRequest request, CancellationToken ct = default);

    Task<OrdemDto> ProporSolucaoAsync(Guid userId, Role role, Guid ordemId, PropostaSolucaoRequest request, CancellationToken ct = default);

    /// <summary>Algoritmo C: resposta do solicitante à proposta (aceite ou recusa com motivo).</summary>
    Task<OrdemDto> ResponderValidacaoAsync(Guid userId, Guid ordemId, ResponderValidacaoRequest request, CancellationToken ct = default);

    Task<OrdemDto> GuardarSolucaoSugeridaAsync(Guid userId, Role role, Guid ordemId, SolucaoSugeridaRequest request, CancellationToken ct = default);
}
