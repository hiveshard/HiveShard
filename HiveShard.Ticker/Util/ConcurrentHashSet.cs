using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HiveShard.Ticker.Util;

public class ConcurrentHashSet<T>
{
    private readonly ConcurrentDictionary<T, byte> _dict;

    public ConcurrentHashSet()
        : this(EqualityComparer<T>.Default) { }
    public ConcurrentHashSet(IEqualityComparer<T> comparer)
        => _dict = new ConcurrentDictionary<T, byte>(comparer);

    public bool Add(T item) => _dict.TryAdd(item, 0);
    public bool Remove(T item) => _dict.TryRemove(item, out _);
    public bool Contains(T item) => _dict.ContainsKey(item);
    public int Count => _dict.Count;
}
