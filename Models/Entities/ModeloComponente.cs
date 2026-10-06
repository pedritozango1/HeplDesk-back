namespace Novati.API.Models.Entities;
/// <summary>Modelo de componente (ex.: "RAM 8GB DDR4").</summary>
public class ModeloComponente : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } =  string.Empty;
    public string Capacidade { get; set; } =  string.Empty;
    public int StockMinimo { get; set; } = 1;
}
