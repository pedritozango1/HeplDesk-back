namespace Novati.API.Common.Settings;

/// <summary>Configuração do armazenamento de ficheiros (secção "Storage" do appsettings).</summary>
public class StorageSettings
{
    /// <summary>Pasta raiz. Relativa → dentro da pasta do projeto (ContentRoot).</summary>
    public string RootPath { get; set; } = "Storage";

    /// <summary>Tamanho máximo por ficheiro, em bytes.</summary>
    public long MaxBytes { get; set; } = 10 * 1024 * 1024;
}
