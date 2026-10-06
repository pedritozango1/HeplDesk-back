using Novati.API.Dtos.Chat;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class MensagemMapper
{
    public static MensagemDto ToDto(Mensagem m) => new(m.Id, m.SolicitacaoId, m.AutorId, m.Texto, m.Data, m.Hora.ToString("HH:mm"), m.Lida);

    public static Mensagem ToEntity(CreateMensagemRequest r, Guid autorId) => new()
    {
        SolicitacaoId = r.SolicitacaoId,
        AutorId = autorId,
        Texto = r.Texto,
        Data = DateOnly.FromDateTime(DateTime.UtcNow),
        Hora = TimeOnly.FromDateTime(DateTime.UtcNow),
        Lida = false,
    };
}
