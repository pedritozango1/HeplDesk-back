using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Novati.API.Common.Settings;
using Novati.API.Dtos.AcessoRemoto;

namespace Novati.API.Services;

/// <summary>
/// Lista de servidores ICE (STUN/TURN) que os browsers usam para se encontrarem.
/// O STUN é público e gratuito; o TURN só entra se estiver configurado.
/// </summary>
public class ServidoresIceService(IOptions<AcessoRemotoSettings> opcoes)
{
    public List<IceServerDto> Obter(Guid userId)
    {
        var cfg = opcoes.Value;
        var lista = new List<IceServerDto>();

        if (cfg.Stun.Length > 0)
            lista.Add(new IceServerDto(cfg.Stun));

        var turn = cfg.Turn;
        if (turn.Urls.Length > 0 && !string.IsNullOrWhiteSpace(turn.Segredo))
        {
            // Credenciais temporárias (coturn, "use-auth-secret"): o utilizador é "<validade>:<id>"
            // e a palavra-passe é o HMAC-SHA1 disso com o segredo. O coturn refaz a conta e
            // recusa depois da validade — o segredo em si nunca sai do servidor.
            var validade = DateTimeOffset.UtcNow.AddSeconds(turn.TtlSegundos).ToUnixTimeSeconds();
            var username = $"{validade}:{userId:N}";
            var credential = Convert.ToBase64String(
                HMACSHA1.HashData(Encoding.UTF8.GetBytes(turn.Segredo), Encoding.UTF8.GetBytes(username)));

            lista.Add(new IceServerDto(turn.Urls, username, credential));
        }

        return lista;
    }
}
