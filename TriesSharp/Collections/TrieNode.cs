using System.Buffers;
using System.Diagnostics;

namespace TriesSharp.Collections;

[DebuggerDisplay("{HasValue ? Value?.ToString() : \"<No value>\"}, Count = {ChildrenCount}")]
[DebuggerTypeProxy(typeof(TrieNode<>.DebuggerProxy))]
internal sealed class TrieNode<TValue>
{

    public TValue Value { get; private set; } = default!;

    public bool HasValue { get; private set; }

    private SortedListLite<char, TrieNode<TValue>> children;

    public TrieNode()
    {
        children.Clear();
    }

    public void SetValue(TValue value)
    {
        Value = value;
        HasValue = true;
    }

    public bool UnsetValue()
    {
        if (!HasValue) return false;
        HasValue = false;
        Value = default!;
        return true;
    }

    public TrieNode<TValue>? TryGetChild(char c)
    {
        return children.TryGetValue(c, out var node) ? node : null;
    }

    public TrieNode<TValue>? TryGetChild(ReadOnlySpan<char> segment)
    {
        var current = this;
        foreach (var c in segment)
        {
            current = current.TryGetChild(c);
            if (current == null) return null;
        }
        return current;
    }

    public TrieNode<TValue> GetOrAddChild(char c)
    {
        return children.GetOrAddDefault(c);
    }

    public TrieNode<TValue> GetOrAddChild(ReadOnlySpan<char> segment)
    {
        var current = this;
        foreach (var c in segment)
            current = current.GetOrAddChild(c);
        return current;
    }

    public bool RemoveChild(char c)
    {
        return children.Remove(c);
    }

    public void ClearChildren()
    {
        children.Clear();
    }

    public int ChildrenCount => children.Count;

    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumDescendants(char[]? keyBuffer, int prefixLength)
    {
        // n.b. keyBuffer will always be the same old keyBuffer reference ever time user re-iterates.
        var reallocatedBuffer = false;
        try
        {
            Debug.Assert(this.children.Count > 0 || this.HasValue, "If the node has no value and no child, it shouldn't be exist.");
            if (this.HasValue)
            {
                yield return KeyValuePair.Create(keyBuffer == null ? default : new ReadOnlyMemory<char>(keyBuffer, 0, prefixLength), this.Value);
            }
            // Shortcut: we may not need to assign new memory for keys.
            if (this.children.Count == 0) yield break;
            var nodeStack = new Stack<TrieNode<TValue>>();
            var childIndexStack = new Stack<int>();
            nodeStack.Push(this);
            childIndexStack.Push(0);
            while (nodeStack.Count > 0)
            {
                var current = nodeStack.Peek();
                var i = childIndexStack.Pop();
                if (i >= current.children!.Count)
                {
                    nodeStack.Pop();
                    continue;
                }
                childIndexStack.Push(i + 1);
                var child = current.children.GetValueAt(i);
                var keyCharIndex = prefixLength + nodeStack.Count - 1;
                if (keyBuffer != null)
                {
                    if (keyCharIndex >= keyBuffer.Length)
                    {
                        var newBuffer = ArrayPool<char>.Shared.Rent(keyBuffer.Length * 2);
                        keyBuffer.CopyTo(newBuffer, 0);
                        ArrayPool<char>.Shared.Return(keyBuffer);
                        keyBuffer = newBuffer;
                        reallocatedBuffer = true;
                    }
                    keyBuffer[keyCharIndex] = current.children.GetKeyAt(i);
                }
                if (child.HasValue)
                {
                    yield return KeyValuePair.Create(keyBuffer == null
                        ? default
                        : new ReadOnlyMemory<char>(keyBuffer, 0, keyCharIndex + 1), child.Value);
                }
                if (child.children is { Count: > 0 })
                {
                    nodeStack.Push(child);
                    childIndexStack.Push(0);
                }
            }
        }
        finally
        {
            if (reallocatedBuffer)
            {
                Debug.Assert(keyBuffer != null);
                ArrayPool<char>.Shared.Return(keyBuffer);
            }
        }
    }

    public void TrimExcess()
    {
        this.children.TrimExcess();
        if (this.children.Count == 0) return;
        var nodeStack = new Stack<TrieNode<TValue>>();
        var childIndexStack = new Stack<int>();
        nodeStack.Push(this);
        childIndexStack.Push(0);
        while (nodeStack.Count > 0)
        {
            var current = nodeStack.Peek();
            var i = childIndexStack.Pop();
            if (i >= current.children.Count)
            {
                nodeStack.Pop();
                continue;
            }
            childIndexStack.Push(i + 1);
            var child = current.children.GetValueAt(i);
            if (child.children.Count > 0)
            {
                child.children.TrimExcess();
                nodeStack.Push(child);
                childIndexStack.Push(0);
            }
        }
    }

    private sealed class DebuggerProxy
    {
        private readonly TrieNode<TValue> node;

        public DebuggerProxy(TrieNode<TValue> node)
        {
            this.node = node;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IReadOnlyCollection<KeyValuePair<char, TrieNode<TValue>>> Children => node.children;
    }

}