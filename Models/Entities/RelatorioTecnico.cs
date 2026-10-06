using Novati.API.Models.Enums;

namespace Novati.API.Models.Entities;

/// <summary>Relatório técnico formal de uma ordem resolvida.</summary>
public class RelatorioTecnico : BaseEntity
{
    public DateOnly CriadoEm { get; set; }
    public DateOnly AtualizadoEm { get; set; }
    public StatusRelatorio Status { get; set; } = StatusRelatorio.rascunho;

    public string Sumario { get; set; } = "";
    public string Diagnostico { get; set; } = "";
    public string SolucaoAplicada { get; set; } = "";
    public int? TempoGastoMin { get; set; }
    public string Procedimentos { get; set; } = "";
    public string Observacoes { get; set; } = "";
    public string AssinaturaTecnico { get; set; } = "";
    public string AssinaturaResponsavel { get; set; } = "";
    public string ComentariosInternos { get; set; } = "";

    // Cópia da assinatura (imagem) do técnico no momento em que finalizou. Fica congelada:
    // se ele mudar a assinatura no perfil, este documento não muda.
    public Guid? AssinaturaTecnicoFicheiroId { get; set; }
    public Ficheiro? AssinaturaTecnicoFicheiro { get; set; }

    // Lista JSONB
    public List<PecaRelatorio> PecasUsadas { get; set; } = [];

    // FK obrigatória 1–1 para OrdemReparo
    public Guid OrdemId { get; set; }
    public OrdemReparo Ordem { get; set; } = null!;

    // FK obrigatória para User (autor)
    public Guid AutorId { get; set; }
    public User Autor { get; set; } = null!;

    // Navegação inversa
    public List<RelatorioHistorico> Historico { get; set; } = [];
}