namespace Novati.API.Models.Enums;

/// <summary>
/// O que o técnico pode fazer durante a sessão remota.
/// </summary>
/// <remarks>
/// Os nomes dos membros correspondem exatamente ao texto que o front-end compara.
/// </remarks>
public enum ModoAcessoRemoto
{
    /// <summary>Só vê o ecrã que o solicitante partilha (browser a browser, sem instalar nada).</summary>
    VER,

    /// <summary>
    /// Controla rato e teclado através do Agente Novati, o nosso programa a correr no PC do
    /// solicitante. O agente só obedece a este servidor e só durante uma sessão ATIVA.
    /// </summary>
    CONTROLAR,
}
