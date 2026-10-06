namespace Novati.API.Models.Entities;

/// <summary>
/// Ficheiro carregado para a API (assinaturas, anexos, ...). A BD guarda só os
/// metadados; os bytes ficam no armazenamento (<see cref="Services.Interfaces.IFileStorage"/>).
/// </summary>
public class Ficheiro : BaseEntity
{
    /// <summary>Nome original enviado pelo cliente (só para mostrar/descarregar).</summary>
    public string NomeOriginal { get; set; } = string.Empty;

    /// <summary>Tipo detetado pelo servidor a partir dos bytes — nunca o que o cliente declarou.</summary>
    public string ContentType { get; set; } = string.Empty;

    public long TamanhoBytes { get; set; }

    /// <summary>Caminho dentro do armazenamento (ex: 2026/09/{guid}.png). Nunca exposto ao cliente.</summary>
    public string CaminhoRelativo { get; set; } = string.Empty;

    /// <summary>SHA-256 em hexadecimal — integridade (a assinatura de um relatório não pode mudar).</summary>
    public string Sha256 { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; }

    // FK para User (quem carregou). Fica null se o utilizador for apagado — o ficheiro
    // continua a existir porque pode estar congelado num relatório.
    public Guid? CriadoPorId { get; set; }
    public User? CriadoPor { get; set; }
}
