using Novati.API.Dtos.AcessoRemoto;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class SessaoRemotaMapper
{
    // TokenHash, tentativas e IP ficam de fora de propósito: não saem da API.
    public static SessaoRemotaDto ToDto(SessaoRemota s) => new(
        s.Id, s.SolicitacaoId, s.TecnicoId, s.SolicitanteId,
        s.Estado.ToString(), s.Modo.ToString(),
        s.PedidaEm, s.RespondidaEm, s.TokenExpiraEm, s.IniciadaEm, s.TerminadaEm,
        s.TerminadaPorId, s.MotivoFim, s.AgentePc
    );
}
