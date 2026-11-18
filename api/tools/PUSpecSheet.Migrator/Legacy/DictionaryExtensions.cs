namespace PUSpecSheet.Migrator.Legacy;

internal static class DictionaryExtensions
{
    /// <summary>
    /// The value for the key, added new if the map has none yet.
    /// </summary>
    public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> map, TKey key)
        where TKey : notnull
        where TValue : new()
    {
        if (!map.TryGetValue(key, out var value))
        {
            value = new TValue();
            map[key] = value;
        }

        return value;
    }
}
