using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriesSharp.Collections;

internal sealed class TrieNode<TValue>
{

    public TValue Value { get; private set; } = default!;

    public bool HasValue { get; private set; }

    private SortedList<char, TrieNode<TValue>>? children;

    public void SetValue(TValue value)
    {
        Value = value;
        HasValue = true;
    }

    // This function is seldom called. We often remove the whole node from parent.
    public bool UnsetValue()
    {
        if (HasValue) return false;
        HasValue = false;
        Value = default!;
        return true;
    }

    public TrieNode<TValue>? TryGetChild(char c)
    {
        if (children == null) return null;
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
        if (children == null) children = new SortedList<char, TrieNode<TValue>>(2);
        if (!children.TryGetValue(c, out var node))
        {
            node = new TrieNode<TValue>();
            children.Add(c, node);
        }
        return node;
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
        if (children == null || !children.Remove(c)) return false;
        if (children.Count == 0) children = null;
        return true;
    }

    public void ClearChildren()
    {
        children = null;
    }

    public int ChildrenCount => children?.Count ?? 0;

    public IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumDescendants(char[]? keyBuffer, int prefixLength)
    {
        // n.b. keyBuffer will always be the same old keyBuffer reference ever time user re-iterates.
        var reallocatedBuffer = false;
        try
        {
            if (this.children == null)
            {
                // Shortcut: we don't need to assign new memory for keys.
                Debug.Assert(this.HasValue, "If the node has no value and no child, it shouldn't be exist.");
                yield return KeyValuePair.Create(keyBuffer == null ? default : new ReadOnlyMemory<char>(keyBuffer, 0, prefixLength), this.Value);
                yield break;
            }
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
                var child = current.children.Values[i];
                if (keyBuffer != null)
                {
                    if (prefixLength + i >= keyBuffer.Length)
                    {
                        var newBuffer = ArrayPool<char>.Shared.Rent(keyBuffer.Length * 2);
                        keyBuffer.CopyTo(newBuffer, 0);
                        ArrayPool<char>.Shared.Return(keyBuffer);
                        keyBuffer = newBuffer;
                        reallocatedBuffer = true;
                    }
                    keyBuffer[prefixLength + i] = current.children.Keys[i];
                }
                if (child.HasValue)
                {
                    yield return KeyValuePair.Create(keyBuffer == null
                        ? default
                        : new ReadOnlyMemory<char>(keyBuffer, 0, prefixLength + i + 1), child.Value);
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

}