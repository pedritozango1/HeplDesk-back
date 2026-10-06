using Npgsql;

namespace Novati.API.Common.Extensions;

/// <summary>Extensões de leitura da configuração (appsettings + variáveis de ambiente).</summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Connection string da base de dados (<c>ConnectionStrings:DefaultConnection</c>).
    /// Aceita o formato do Npgsql ("Host=...;Database=...") e também o URL que os serviços
    /// de alojamento dão ("postgresql://user:pass@host/db?sslmode=require"), que o Npgsql
    /// não entende diretamente.
    /// </summary>
    public static string GetLigacaoBd(this IConfiguration config)
    {
        var valor = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");

        if (!valor.StartsWith("postgres://") && !valor.StartsWith("postgresql://"))
            return valor;

        var uri = new Uri(valor);
        var credenciais = uri.UserInfo.Split(':', 2);

        // ?sslmode=require|disable|verify-full … — sem o parâmetro, exige SSL (é um servidor remoto).
        var sslTexto = System.Web.HttpUtility.ParseQueryString(uri.Query)["sslmode"]?.Replace("-", "");
        var ssl = Enum.TryParse<SslMode>(sslTexto, ignoreCase: true, out var modo) ? modo : SslMode.Require;

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credenciais[0]),
            Password = credenciais.Length > 1 ? Uri.UnescapeDataString(credenciais[1]) : null,
            SslMode = ssl,
        }.ConnectionString;
    }

    /// <summary>
    /// Dados e contas de demonstração: sempre em Development; noutros ambientes só com
    /// <c>Seed:Demo=true</c> (variável de ambiente <c>Seed__Demo</c>).
    /// </summary>
    public static bool ModoDemo(this IConfiguration config, IHostEnvironment env) =>
        env.IsDevelopment() || config.GetValue<bool>("Seed:Demo");
}
