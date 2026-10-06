namespace Novati.API.Common.Settings;

/// <summary>
/// Listas de domínio configuráveis (secção "Dominio" do appsettings.json) —
/// mudam-se sem recompilar o front nem o back-end.
/// </summary>
public class DominioSettings
{
    /// <summary>Categorias oferecidas ao abrir uma solicitação.</summary>
    public List<string> Categorias { get; set; } = [];

    /// <summary>Tipos de modelo de dispositivo (catálogo).</summary>
    public List<string> TiposDispositivo { get; set; } = [];

    /// <summary>Tipos de modelo de componente (catálogo).</summary>
    public List<string> TiposComponente { get; set; } = [];

    /// <summary>Horas até uma solicitação em aberto ficar fora do prazo, por prioridade (BAIXA, MEDIA, …).</summary>
    public Dictionary<string, int> SlaHoras { get; set; } = [];
}
