using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;

/// <summary>Entrada no histórico/timeline de uma ordem.</summary>
public class OrdemHistorico : BaseEntity
{
    public TipoHistoricoOrdem Tipo { get; set; }
    public string Texto { get; set; } = "";
    public DateOnly Data { get; set; }

    /// <summary>Posição na timeline da ordem (0, 1, 2…). Desempata entradas do mesmo dia.</summary>
    public int Posicao { get; set; }

    public Guid OrdemId { get; set; }
    public OrdemReparo Ordem { get; set; } = null!;

    // Autor opcional (entradas automáticas podem não ter autor)
    public Guid? AutorId { get; set; }
    public User? Autor { get; set; }
}