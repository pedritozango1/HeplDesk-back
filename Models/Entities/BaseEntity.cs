namespace Novati.API.Models.Entities;
/// <summary>
/// Classe base de todas as entidades persistidas na base de dados.
/// Fornece a chave primária (Guid) comum a todas as tabelas.
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// Identificador único da entidade. Gerado automaticamente ao criar a instância.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}
