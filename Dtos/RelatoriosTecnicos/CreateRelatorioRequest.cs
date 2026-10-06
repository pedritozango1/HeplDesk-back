using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.RelatoriosTecnicos;

/// <summary>Criação de um relatório técnico a partir de uma ordem RESOLVIDO.</summary>
public class CreateRelatorioRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid OrdemId { get; set; }

    public string Sumario { get; set; } = "";
    public string Diagnostico { get; set; } = "";
    public string SolucaoAplicada { get; set; } = "";
    public List<PecaRelatorioDto> PecasUsadas { get; set; } = [];
    public string Procedimentos { get; set; } = "";
    public string Observacoes { get; set; } = "";
    public string AssinaturaTecnico { get; set; } = "";
    public string AssinaturaResponsavel { get; set; } = "";
    public string ComentariosInternos { get; set; } = "";
}
