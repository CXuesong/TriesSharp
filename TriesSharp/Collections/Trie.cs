using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TriesSharp.Collections;

public partial class Trie<TValue> : ITrie<TValue>
{
    private readonly TrieNode<TValue> root;
    private int _Count;
    private int _LongestPossibleKeyLength;
    private const int EnumerableMaxKeyBufferLength = 512;

    /// <inheritdoc />
    public void Add(ReadOnlySpan<char> key, TValue value)
    {
        var node = root.GetOrAddChild(key);
        if (node.HasValue) throw new ArgumentException("Specified key already exists.", nameof(key));
        node.SetValue(value);
        _Count++;
        _LongestPossibleKeyLength = Math.Max(_LongestPossibleKeyLength, key.Length);
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
        if (key.IsEmpty)
        {
            if (root.UnsetValue())
            {
                _Count--;
                Debug.Assert(_Count >= 0);
                return true;
            }
            return false;
        }
        var opParentNode = root;
        var opChildKey = '\0';
        var currentNode = root;
        foreach (var c in key)
        {
            currentNode = currentNode.TryGetChild(c);
            if (currentNode == null) return false;
            if (currentNode.HasValue || currentNode.ChildrenCount > 1)
            {
                opParentNode = currentNode;
                opChildKey = c;
            }
        }
        var result = opParentNode.RemoveChild(opChildKey);
        Debug.Assert(result);
        _Count--;
        // _PossibleLongestKeyLength may be inaccurate from this point of time.
        Debug.Assert(_Count >= 0);
        return result;
    }

    /// <inheritdoc />
    public bool Remove(ReadOnlyMemory<char> key) => Remove(key.Span);

    /// <inheritdoc />
    public bool Remove(string key) => Remove(key.AsSpan());

    /// <inheritdoc />
    public void Clear()
    {
        root.ClearChildren();
        root.UnsetValue();
        _Count = 0;
        _LongestPossibleKeyLength = 0;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<ReadOnlyMemory<char>, TValue>> GetEnumerator()
    {
        if (this._Count == 0) yield break;
        var buffer = ArrayPool<char>.Shared.Rent(Math.Min(this._LongestPossibleKeyLength, EnumerableMaxKeyBufferLength));
        try
        {
            foreach (var item in this.root.EnumDescendants(buffer, 0))
                yield return item;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.Contains(KeyValuePair<ReadOnlyMemory<char>, TValue> item)
        => TryGetValue(item.Key.Span, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<ReadOnlyMemory<char>, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (arrayIndex + _Count > array.Length) throw new ArgumentException("Target array does not have sufficient space available.");
        var i = arrayIndex;
        foreach (var (key, value) in this)
        {
            array[i] = KeyValuePair.Create(new ReadOnlyMemory<char>(key.ToArray()), value);
            i++;
        }
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
    public int Count => _Count;

    /// <inheritdoc />
    public bool ContainsKey(ReadOnlySpan<char> key)
    {
        var node = root.TryGetChild(key);
        return node is { HasValue: true };
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey" />
    public bool ContainsKey(ReadOnlyMemory<char> key) => ContainsKey(key.Span);

    /// <inheritdoc />
    public ICollection<ReadOnlyMemory<char>> Keys { get; } = new KeyCollection();

    /// <inheritdoc />
    public ICollection<TValue> Values { get; } = new ValueCollection();

    /// <inheritdoc cref="ITrie{TValue}.this[ReadOnlySpan{char}]" />
    public TValue this[ReadOnlySpan<char> key]
    {
        get
        {
            if (key.Length > _LongestPossibleKeyLength) throw new KeyNotFoundException();
            var node = root.TryGetChild(key);
            if (node is not { HasValue: true }) throw new KeyNotFoundException();
            return node.Value;
        }
        set
        {
            var node = root.GetOrAddChild(key);
            node.SetValue(value);
        }
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
    public TValue this[ReadOnlyMemory<char> key]
    {
        get => this[key.Span];
        set => this[key.Span] = value;
    }

    /// <inheritdoc cref="ITrie{TValue}.this[string]" />
    public TValue this[string key] { get => this[key.AsSpan()]; set => this[key.AsSpan()] = value; }

    /// <inheritdoc />
    public bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;
        if (key.Length > _LongestPossibleKeyLength) return false;
        var node = root.TryGetChild(key);
        if (node is not { HasValue: true }) return false;
        value = node.Value;
        return true;
    }

    /// <inheritdoc cref="ITrie{TValue}.TryGetValue(ReadOnlyMemory{char},out TValue)" />
    public bool TryGetValue(ReadOnlyMemory<char> key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key.Span, out value);

    /// <inheritdoc />
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key.AsSpan(), out value);

    /// <inheritdoc />
    public (int PrefixLength, TValue value) MatchLongestPrefix(ReadOnlySpan<char> query)
    {
        var current = root;
        var (lastMatchPrefixLength, lastMatch) = root.HasValue ? (0, root) : (-1, null);
        for (var i = 0; i < query.Length; i++)
        {
            current = current.TryGetChild(query[i]);
            if (current == null) break;
            if (current.HasValue) (lastMatchPrefixLength, lastMatch) = (i + 1, current);
        }
        return (lastMatchPrefixLength, lastMatch == null ? default! : lastMatch.Value);
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(ReadOnlySpan<char> prefix)
    {
        if (prefix.Length > _LongestPossibleKeyLength) return Enumerable.Empty<KeyValuePair<ReadOnlyMemory<char>, TValue>>();
        var node = root.TryGetChild(prefix);
        if (node == null) return Enumerable.Empty<KeyValuePair<ReadOnlyMemory<char>, TValue>>();
        var keyLength = Math.Max(Math.Min(_LongestPossibleKeyLength, EnumerableMaxKeyBufferLength), prefix.Length + 1);
        var buffer = GC.AllocateUninitializedArray<char>(keyLength);
        prefix.CopyTo(buffer.AsSpan());
        return node.EnumDescendants(buffer, prefix.Length);
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