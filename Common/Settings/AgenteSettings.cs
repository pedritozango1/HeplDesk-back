namespace Novati.API.Common.Settings;

/// <summary>Configuração da distribuição do Agente Novati (secção "Agente" do appsettings).</summary>
public class AgenteSettings
{
    /// <summary>Onde está o executável do agente. Relativo → dentro da pasta do projeto (ContentRoot). Gerado por agente\publicar.ps1.</summary>
    public string CaminhoExe { get; set; } = "Agente/NovatiAgente.exe";

    /// <summary>
    /// Endereço público desta API (ex.: https://api.empresa.com), gravado no agente descarregado.
    /// Vazio → usa o endereço por onde o pedido de download chegou.
    /// </summary>
    public string UrlPublica { get; set; } = "";

    /// <summary>Validade de um link de download.</summary>
    public int LinkTtlHoras { get; set; } = 72;
}
