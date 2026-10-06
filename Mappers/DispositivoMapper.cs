using Novati.API.Dtos.Dispositivos;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

/// <summary>Converte as entidades de inventário (Localizacao, DispositivoFisico, InstanciaComponente) ⇄ DTO.</summary>
public static class DispositivoMapper
{
    public static LocalizacaoDto ToDto(Localizacao l) => new(l.Id, l.Nome, l.PaiId);

    public static DispositivoDto ToDto(DispositivoFisico d) => new(
        d.Id, d.Patrimonio, d.ModeloDispositivoId, d.NumeroSerie,
        d.LocalizacaoId, d.ResponsavelId, d.Estado.ToString(),
        d.DataAquisicao, d.GarantiaMeses
    );

    public static InstanciaDto ToDto(InstanciaComponente i) => new(
        i.Id, i.DispositivoFisicoId, i.ModeloComponenteId, i.Codigo, i.Estado.ToString()
    );

    public static Localizacao ToEntity(CreateLocalizacaoRequest r) => new()
    {
        Nome = r.Nome,
        PaiId = r.Pai,
    };

    public static DispositivoFisico ToEntity(CreateDispositivoRequest r) => new()
    {
        Patrimonio = r.Patrimonio,
        ModeloDispositivoId = r.ModeloDispositivoId,
        NumeroSerie = r.NumeroSerie ?? "",
        LocalizacaoId = r.LocalizacaoId,
        ResponsavelId = r.ResponsavelId,
        DataAquisicao = r.DataAquisicao,
        GarantiaMeses = r.GarantiaMeses,
    };

    public static InstanciaComponente ToEntity(Guid dispositivoFisicoId, CreateInstanciaRequest r) => new()
    {
        DispositivoFisicoId = dispositivoFisicoId,
        ModeloComponenteId = r.ModeloComponenteId,
        Codigo = r.Codigo,
    };
}
