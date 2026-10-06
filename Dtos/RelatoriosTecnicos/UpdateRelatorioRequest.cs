using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.RelatoriosTecnicos;

/// <summary>
/// Atualização de um relatório. Só os campos enviados são alterados: um campo em falta
/// (<c>null</c>) é considerado "não enviado" e não entra no diff do histórico.
/// </summary>
public class UpdateRelatorioRequest
{
    public string? Sumario { get; set; }
    public string? Diagnostico { get; set; }
    public string? SolucaoAplicada { get; set; }
    public List<PecaRelatorioDto>? PecasUsadas { get; set; }
    public string? Procedimentos { get; set; }
    public string? Observacoes { get; set; }
    public string? AssinaturaTecnico { get; set; }
    public string? AssinaturaResponsavel { get; set; }
    public string? ComentariosInternos { get; set; }
}
