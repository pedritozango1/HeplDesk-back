namespace Novati.API.Models.Enums;

/// <summary>
/// Estado de uma <c>InstanciaComponente</c> (um componente físico concreto instalado num dispositivo).
/// </summary>
/// <remarks>
/// Tem atualmente um único valor possível no domínio do front-end; existe como enum (em vez de
/// campo fixo) para permitir novos estados no futuro sem alterar o tipo da coluna.
/// </remarks>
public enum EstadoInstancia
{
    /// <summary>Componente montado e a funcionar no dispositivo físico.</summary>
    INSTALADO,
}
