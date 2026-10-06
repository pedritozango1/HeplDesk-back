using System.Collections.Concurrent;

namespace Novati.API.Realtime;

/// <summary>
/// Quem está ligado neste momento (contagem de ligações por utilizador — o mesmo
/// utilizador pode ter vários separadores abertos). Singleton, só em memória.
/// </summary>
public class PresencaTracker
{
    private readonly ConcurrentDictionary<Guid, int> _ligacoes = new();

    /// <summary>Regista uma ligação; devolve true se o utilizador acabou de ficar online.</summary>
    public bool Ligou(Guid userId)
        => _ligacoes.AddOrUpdate(userId, 1, (_, n) => n + 1) == 1;

    /// <summary>Remove uma ligação; devolve true se o utilizador ficou offline.</summary>
    public bool Desligou(Guid userId)
    {
        while (_ligacoes.TryGetValue(userId, out var n))
        {
            if (n <= 1)
            {
                if (_ligacoes.TryRemove(new KeyValuePair<Guid, int>(userId, n))) return true;
            }
            else if (_ligacoes.TryUpdate(userId, n - 1, n))
            {
                return false;
            }
        }
        return false;
    }

    public IReadOnlyCollection<Guid> Online => _ligacoes.Keys.ToList();
}
