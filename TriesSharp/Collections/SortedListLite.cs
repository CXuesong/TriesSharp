﻿using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TriesSharp.Collections;

/// <summary>
/// A home-made SortedList counterpart to reduce memory allocation.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
internal struct SortedListLite<TKey, TValue> : IReadOnlyList<KeyValuePair<TKey, TValue>>
    where TValue : new()
{
    private const int DefaultCapacity = 4;
    private TKey[] keys;
    private TValue[] values;
    private int count;

    public void Clear()
    {
        keys = Array.Empty<TKey>();
        values = Array.Empty<TValue>();
        count = 0;
    }

    public int Count => count;

    public int Capacity
    {
        get => keys.Length;
        set
        {
            Debug.Assert(value >= count);
            if (value == keys.Length) return;
            if (value > 0)
            {
                var newKeys = GC.AllocateUninitializedArray<TKey>(value);
                var newValues = GC.AllocateUninitializedArray<TValue>(value);
                Array.Copy(keys, newKeys, count);
                Array.Copy(values, newValues, count);
                keys = newKeys;
                values = newValues;
            }
            else
            {
                keys = Array.Empty<TKey>();
                values = Array.Empty<TValue>();
            }
        }
    }

    public void TrimExcess()
    {
        if (keys.Length - count >= keys.Length / 10)
            Capacity = count;
    }

    public TValue GetOrAddDefault(TKey key)
    {
        var index = Array.BinarySearch(keys, 0, count, key);
        if (index < 0)
        {
            index = ~index;
            InsertAt(index, key, new TValue());
        }
        return values[index];
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        Debug.Assert(keys.Length == values.Length);
        var index = Array.BinarySearch(keys, 0, count, key);
        if (index < 0)
        {
            value = default;
            return false;
        }
        value = values[index];
        return true;
    }

    public bool Remove(TKey key)
    {
        Debug.Assert(keys.Length == values.Length);
        var index = Array.BinarySearch(keys, 0, count, key);
        if (index < 0) return false;
        count--;
        if (index < count)
        {
            // count = 6
            // index = 4
            // 0 1 2 3 4 5
            //         x
            Array.Copy(keys, index + 1, keys, index, count - index);
            Array.Copy(values, index + 1, values, index, count - index);
        }
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>()) keys[count] = default!;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>()) values[count] = default!;
        return true;
    }

    private void InsertAt(int index, TKey key, TValue value)
    {
        Debug.Assert(keys.Length == values.Length);
        if (keys.Length <= count)
        {
            // Extend storage.
            Capacity = Math.Max(DefaultCapacity, Capacity * 2);
        }
        if (index < count)
        {
            // count = 6
            // index = 4
            // 0 1 2 3 | 4 5
            Array.Copy(keys, index, keys, index + 1, count - index);
            Array.Copy(values, index, values, index + 1, count - index);
        }
        keys[index] = key;
        values[index] = value;
        count++;
    }

    public TKey GetKeyAt(int index)
    {
        Debug.Assert(index < count);
        return keys[index];
    }

    public TValue GetValueAt(int index)
    {
        Debug.Assert(index < count);
        return values[index];
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
            yield return KeyValuePair.Create(keys[i], values[i]);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public KeyValuePair<TKey, TValue> this[int index] => new (keys[index], values[index]);
}