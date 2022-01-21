using System.Collections;

namespace TriesSharp.Collections;

public partial class Trie<TValue> : ITrie<TValue>
{
    private readonly TrieNode<TValue> root;

    /// <inheritdoc />
    public void Add(ReadOnlySpan<char> key, TValue value)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Add(ReadOnlyMemory<char> key, TValue value)
        => Add(key.Span, value);

    /// <inheritdoc />
    public void Add(string key, TValue value)
        => Add(key.AsSpan(), value);

    /// <inheritdoc />
    public void Add(KeyValuePair<ReadOnlyMemory<char>, TValue> item)
        => Add(item.Key.Span, item.Value);

    /// <inheritdoc />
    public bool Remove(ReadOnlySpan<char> key)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool Remove(ReadOnlyMemory<char> key) => Remove(key.Span);

    /// <inheritdoc />
    public bool Remove(string key) => Remove(key.AsSpan());

    /// <inheritdoc />
    public void Clear()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<ReadOnlyMemory<char>, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.Contains(KeyValuePair<ReadOnlyMemory<char>, TValue> item)
        => TryGetValue(item.Key.Span, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<ReadOnlyMemory<char>, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.Remove(KeyValuePair<ReadOnlyMemory<char>, TValue> item)
    {
        if (TryGetValue(item.Key.Span, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value))
        {
            return Remove(item.Key);
        }
        return false;
    }

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => throw new NotImplementedException();

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey" />
    public bool ContainsKey(ReadOnlyMemory<char> key)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ICollection<ReadOnlyMemory<char>> Keys { get; } = new KeyCollection();

    /// <inheritdoc />
    public ICollection<TValue> Values { get; } = new ValueCollection();

    /// <inheritdoc cref="ITrie{TValue}.this[ReadOnlySpan{char}]" />
    public TValue this[ReadOnlySpan<char> key]
    {
        get => throw new NotImplementedException(); 
        set => throw new NotImplementedException();
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
    public TValue this[ReadOnlyMemory<char> key] { 
        get => this[key.Span];
        set => this[key.Span] = value;
    }

    /// <inheritdoc cref="ITrie{TValue}.this[string]" />
    public TValue this[string key] { get => this[key.AsSpan()]; set => this[key.AsSpan()] = value; }

    /// <inheritdoc />
    public bool TryGetValue(ReadOnlySpan<char> key, out TValue value)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="ITrie{TValue}.TryGetValue(ReadOnlyMemory{char},out TValue)" />
    public bool TryGetValue(ReadOnlyMemory<char> key, out TValue value) => TryGetValue(key.Span, out value);

    /// <inheritdoc />
    public bool TryGetValue(string key, out TValue value) => TryGetValue(key.AsSpan(), out value);

    /// <inheritdoc />
    public (int PrefixLength, TValue value) MatchLongestPrefix(ReadOnlySpan<char> query)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(ReadOnlySpan<char> prefix)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.IsReadOnly => false;

    /// <inheritdoc />
    IEnumerable<ReadOnlyMemory<char>> IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.Keys => Keys;

    /// <inheritdoc />
    IEnumerable<TValue> IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.Values => Values;

    private sealed class KeyCollection : ICollection<ReadOnlyMemory<char>>
    {
        /// <inheritdoc />
        public IEnumerator<ReadOnlyMemory<char>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(ReadOnlyMemory<char> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(ReadOnlyMemory<char> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyTo(ReadOnlyMemory<char>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Remove(ReadOnlyMemory<char> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count { get; }

        /// <inheritdoc />
        public bool IsReadOnly { get; }
    }

    private sealed class ValueCollection : ICollection<TValue>
    {
        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(TValue item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(TValue item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count { get; }

        /// <inheritdoc />
        public bool IsReadOnly { get; }
    }
}