using Novati.API.Dtos.Catalogo;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

/// <summary>Converte as entidades de catálogo ⇄ DTO.</summary>
public static class CatalogoMapper
{
    public static ModeloDispositivoDto ToDto(ModeloDispositivo m) => new(m.Id, m.Nome, m.Fabricante, m.Tipo);

    public static ModeloComponenteDto ToDto(ModeloComponente m) => new(m.Id, m.Nome, m.Tipo, m.Capacidade, m.StockMinimo);

    public static CompatibilidadeDto ToDto(Compatibilidade c) => new(c.Id, c.ModeloDispositivoId, c.ModeloComponenteId);

    public static ModeloDispositivo ToEntity(CreateModeloDispositivoRequest r) => new()
    {
        Nome = r.Nome,
        Fabricante = r.Fabricante,
        Tipo = r.Tipo,
    };

    public static void ApplyUpdate(ModeloDispositivo m, UpdateModeloDispositivoRequest r)
    {
        m.Nome = r.Nome;
        m.Fabricante = r.Fabricante;
        m.Tipo = r.Tipo;
    }

    public static ModeloComponente ToEntity(CreateModeloComponenteRequest r) => new()
    {
        Nome = r.Nome,
        Tipo = r.Tipo,
        Capacidade = r.Capacidade ?? "",
    };

    public static void ApplyUpdate(ModeloComponente m, UpdateModeloComponenteRequest r)
    {
        m.Nome = r.Nome;
        m.Tipo = r.Tipo;
        m.Capacidade = r.Capacidade ?? "";
    }

    public static Compatibilidade ToEntity(CreateCompatibilidadeRequest r) => new()
    {
        ModeloDispositivoId = r.ModeloDispositivoId,
        ModeloComponenteId = r.ModeloComponenteId,
    };
}
