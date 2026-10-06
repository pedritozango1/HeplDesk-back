using Novati.API.Dtos.Notificacoes;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class NotificacaoMapper
{
    public static NotificacaoDto ToDto(Notificacao n) => new(n.Id, n.UserId, n.Message, n.Link, n.Lida, n.Data);
}
