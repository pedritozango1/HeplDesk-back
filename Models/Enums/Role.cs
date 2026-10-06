namespace Novati.API.Models.Enums;

/// <summary>
/// Perfil de acesso do utilizador. Define o que pode ver/fazer na API (ver F.6 — matriz de permissões).
/// </summary>
/// <remarks>
/// Os nomes dos membros quebram a convenção PascalCase de propósito: correspondem exatamente
/// ao texto que o front-end usa, para que o <c>JsonStringEnumConverter</c> serialize/desserialize
/// sem mapeamento extra.
/// </remarks>
public enum Role
{
    /// <summary>Acesso total: utilizadores, catálogo, dispositivos, stock, etc.</summary>
    ADMIN,

    /// <summary>Gestão: aprova compras, reatribui atendimentos, sem acesso a configuração base.</summary>
    GESTOR,

    /// <summary>Executa atendimentos: diagnostica, repara, usa stock, escreve relatórios técnicos.</summary>
    TECNICO,

    /// <summary>Utilizador final: cria e acompanha as suas próprias solicitações de suporte.</summary>
    FUNCIONARIO,
}
