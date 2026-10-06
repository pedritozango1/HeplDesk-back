using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;
/// <summary>Entrada de auditoria no histórico de um relatório.</summary>
public class RelatorioHistorico : BaseEntity
{
    public DateOnly Data { get; set; }

    /// <summary>Posição na timeline do relatório (0, 1, 2…). Desempata entradas do mesmo dia.</summary>
    public int Posicao { get; set; }
    public AcaoRelatorio Acao { get; set; }
    public string Campo { get; set; } = "";
    public string ValorAntigo { get; set; } = "";
    public string ValorNovo { get; set; } = "";

    public Guid RelatorioId { get; set; }
    public RelatorioTecnico Relatorio { get; set; } = null!;

    public Guid AutorId { get; set; }
    public User Autor { get; set; } = null!;
}
