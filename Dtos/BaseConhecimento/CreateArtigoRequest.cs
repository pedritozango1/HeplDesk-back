using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.BaseConhecimento;

/// <summary>Pedido de criação de um artigo (ADMIN/TECNICO/GESTOR).</summary>
public class CreateArtigoRequest
{
    /// <example>Como ligar-se ao Wi-Fi da empresa</example>
    [Required]
    public string Titulo { get; set; } = "";

    /// <example>Passos: 1) Clique no ícone de rede; 2) Escolha 'EmpresaWiFi'...</example>
    [Required]
    public string Conteudo { get; set; } = "";

    public List<string> Tags { get; set; } = [];

    /// <example>Rede</example>
    [Required]
    public string Categoria { get; set; } = "";
}
