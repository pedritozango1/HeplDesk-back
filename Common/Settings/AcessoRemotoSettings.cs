namespace Novati.API.Common.Settings;

/// <summary>Configuração do acesso remoto (secção "AcessoRemoto" do appsettings).</summary>
public class AcessoRemotoSettings
{
    /// <summary>Tempo que o solicitante tem para responder ao pedido.</summary>
    public int PedidoTtlSegundos { get; set; } = 600;

    /// <summary>Validade do token depois de autorizado.</summary>
    public int TokenTtlSegundos { get; set; } = 300;

    /// <summary>Códigos errados admitidos antes de a sessão expirar.</summary>
    public int MaxTentativas { get; set; } = 5;

    /// <summary>Duração máxima de uma sessão ativa.</summary>
    public int DuracaoMaxMinutos { get; set; } = 60;

    /// <summary>
    /// Servidores STUN (descobrem o endereço público de cada PC). Os públicos são gratuitos.
    /// Sem valor por omissão aqui: a configuração ACRESCENTA a um array já preenchido, não o substitui.
    /// </summary>
    public string[] Stun { get; set; } = [];

    public TurnSettings Turn { get; set; } = new();
}

/// <summary>
/// Servidor TURN (retransmite o vídeo quando os dois PCs não se conseguem ligar diretamente).
/// Opcional: com Urls vazio a API não o anuncia e a ligação é só direta.
/// </summary>
public class TurnSettings
{
    public string[] Urls { get; set; } = [];

    /// <summary>Segredo partilhado com o coturn (static-auth-secret). Nunca no appsettings do repositório.</summary>
    public string Segredo { get; set; } = "";

    /// <summary>Validade das credenciais temporárias entregues aos browsers.</summary>
    public int TtlSegundos { get; set; } = 3600;
}
