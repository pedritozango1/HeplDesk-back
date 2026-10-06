namespace Novati.API.Models.Entities;
/// <summary>Modelo de dispositivo (ex.: "Lenovo ThinkPad E14").</summary>
public class ModeloDispositivo : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } =string.Empty;
    public string Tipo { get; set; } =string.Empty;
}
