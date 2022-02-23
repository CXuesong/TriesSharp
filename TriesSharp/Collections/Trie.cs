using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TriesSharp.Collections;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Trie<>.DebuggerProxy))]
public class Trie<TValue> : ITrie<TValue>
{
    private const int EnumerableMaxKeyBufferLength = 512;

    private readonly TrieNode<TValue> root;
    private int _Count;
    private int _LongestPossibleKeyLength;

    public Trie()
    {
        root = new TrieNode<TValue>();
        Keys = new KeyCollection(this);
        Values = new ValueCollection(this);
    }

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
    void ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.Add(KeyValuePair<ReadOnlyMemory<char>, TValue> item)
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
        var currentNode = root;
        // Last node (excluding the node "key") with value or with more than 1 child.
        TrieNode<TValue>? opParentNode = null;
        var opChildKey = '\0';
        foreach (var c in key)
        {
            var next = currentNode.TryGetChild(c);
            if (next == null) return false;
            if (opParentNode == null || currentNode.HasValue || currentNode.ChildrenCount > 1)
            {
                opParentNode = currentNode;
                opChildKey = c;
            }
            currentNode = next;
        }
        if (currentNode.ChildrenCount > 0)
        {
            // This node has children. Do not remove the node. Try to remove value instead.
            if (!currentNode.UnsetValue()) return false;
        }
        else
        {
            // Remove the node and its branch.
            var result = opParentNode!.RemoveChild(opChildKey);
            Debug.Assert(result);
        }
        _Count--;
        // _PossibleLongestKeyLength may be inaccurate from this point of time.
        Debug.Assert(_Count >= 0);
        return true;
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
    public bool ContainsKey(string key) => ContainsKey(key.AsSpan());

    public bool ContainsValue(TValue value)
    {
        var comparer = EqualityComparer<TValue>.Default;
        foreach (var (_, v) in this.root.EnumDescendants(null, 0))
        {
            if (comparer.Equals(v, value)) return true;
        }
        return false;
    }

    public KeyCollection Keys { get; }

    public ValueCollection Values { get; }

    /// <inheritdoc />
    ICollection<ReadOnlyMemory<char>> IDictionary<ReadOnlyMemory<char>, TValue>.Keys => Keys;

    /// <inheritdoc />
    ICollection<TValue> IDictionary<ReadOnlyMemory<char>, TValue>.Values => Values;

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
            if (!node.HasValue)
            {
                _Count++;
                _LongestPossibleKeyLength = Math.Max(_LongestPossibleKeyLength, key.Length);
            }
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

    /// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue" />
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
    public (int PrefixLength, TValue value) MatchLongestPrefix(ReadOnlyMemory<char> query)
        => MatchLongestPrefix(query.Span);

    /// <inheritdoc />
    public (int PrefixLength, TValue value) MatchLongestPrefix(string query)
        => MatchLongestPrefix(query.AsSpan());

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
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(ReadOnlyMemory<char> prefix)
        => EnumEntriesFromPrefix(prefix.Span);

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(string prefix)
        => EnumEntriesFromPrefix(prefix.AsSpan());

    public void TrimExcess()
    {
        root.TrimExcess();
    }

    /// <inheritdoc />
    bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.IsReadOnly => false;

    /// <inheritdoc />
    IEnumerable<ReadOnlyMemory<char>> IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.Keys => Keys;

    /// <inheritdoc />
    IEnumerable<TValue> IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.Values => Values;

    public sealed class KeyCollection : ICollection<ReadOnlyMemory<char>>
    {
        private readonly Trie<TValue> owner;

        internal KeyCollection(Trie<TValue> owner)
        {
            Debug.Assert(owner != null);
            this.owner = owner;
        }

        /// <inheritdoc />
        public IEnumerator<ReadOnlyMemory<char>> GetEnumerator() 
            => owner.Select(p => p.Key).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        void ICollection<ReadOnlyMemory<char>>.Add(ReadOnlyMemory<char> item) => throw new InvalidOperationException();

        /// <inheritdoc />
        void ICollection<ReadOnlyMemory<char>>.Clear() => throw new InvalidOperationException();

        /// <inheritdoc />
        bool ICollection<ReadOnlyMemory<char>>.Contains(ReadOnlyMemory<char> item) => owner.ContainsKey(item);

        /// <inheritdoc />
        public void CopyTo(ReadOnlyMemory<char>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + owner._Count > array.Length) throw new ArgumentException("Target array does not have sufficient space available.");
            var i = arrayIndex;
            foreach (var (key, _) in owner)
            {
                array[i] = new ReadOnlyMemory<char>(key.ToArray());
                i++;
            }
        }

        /// <inheritdoc />
        bool ICollection<ReadOnlyMemory<char>>.Remove(ReadOnlyMemory<char> item) => throw new InvalidOperationException();

        /// <inheritdoc />
        public int Count => owner._Count;

        /// <inheritdoc />
        bool ICollection<ReadOnlyMemory<char>>.IsReadOnly => false;
    }

    public sealed class ValueCollection : ICollection<TValue>
    {
        private readonly Trie<TValue> owner;

        internal ValueCollection(Trie<TValue> owner)
        {
            this.owner = owner;
        }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator() 
            => owner.root.EnumDescendants(null, 0).Select(p => p.Value).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        void ICollection<TValue>.Add(TValue item) => throw new InvalidOperationException();

        /// <inheritdoc />
        void ICollection<TValue>.Clear() => throw new InvalidOperationException();

        /// <inheritdoc />
        public bool Contains(TValue item) => owner.ContainsValue(item);

        /// <inheritdoc />
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex + owner._Count > array.Length) throw new ArgumentException("Target array does not have sufficient space available.");
            var i = arrayIndex;
            foreach (var (_, value) in owner)
            {
                array[i] = value;
                i++;
            }
        }

        /// <inheritdoc />
        bool ICollection<TValue>.Remove(TValue item) => throw new InvalidOperationException();

        /// <inheritdoc />
        public int Count => owner._Count;

        /// <inheritdoc />
        bool ICollection<TValue>.IsReadOnly => false;
    }

    private sealed class DebuggerProxy
    {
        private readonly Trie<TValue> trie;

        public DebuggerProxy(Trie<TValue> trie)
        {
            this.trie = trie;
        }

        public TrieNode<TValue> Root => trie.root;
    }

}