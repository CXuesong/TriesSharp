﻿using System.Diagnostics.CodeAnalysis;

namespace TriesSharp.Collections;

/// <summary>
/// Represents a collection of string key / value pairs that can be retrieved by prefix. 
/// </summary>
/// <typeparam name="TValue">type of the values.</typeparam>
public interface ITrie<TValue> : IReadOnlyTrie<TValue>, IDictionary<ReadOnlyMemory<char>, TValue>
{
    void Add(ReadOnlySpan<char> key, TValue value);

    void IDictionary<ReadOnlyMemory<char>, TValue>.Add(ReadOnlyMemory<char> key, TValue value) => Add(key.Span, value);

    void Add(string key, TValue value) => Add(key.AsSpan(), value);

    TValue GetOrAdd(ReadOnlySpan<char> key, TValue value)
    {
        if (TryGetValue(key, out var v)) return v;
        Add(key, value);
        return value;
    }

    TValue GetOrAdd(ReadOnlyMemory<char> key, TValue value) => GetOrAdd(key.Span, value);

    TValue GetOrAdd(string key, TValue value) => GetOrAdd(key.AsMemory(), value);

    bool Remove(ReadOnlySpan<char> key);

    bool IDictionary<ReadOnlyMemory<char>, TValue>.Remove(ReadOnlyMemory<char> key) => Remove(key.Span);

    bool Remove(string key) => Remove(key.AsSpan());

    new TValue this[ReadOnlySpan<char> key] { get; set; }

    new TValue this[string key]
    {
        get => this[key.AsSpan()];
        set => this[key.AsSpan()] = value;
    }

    bool IDictionary<ReadOnlyMemory<char>, TValue>.TryGetValue(ReadOnlyMemory<char> key, [MaybeNullWhen(false)] out TValue value)
        => TryGetValue(key.Span, out value);

    TValue IDictionary<ReadOnlyMemory<char>, TValue>.this[ReadOnlyMemory<char> key]
    {
        get => this[key.Span];
        set => this[key.Span] = value;
    }
}

public interface IReadOnlyTrie<TValue> : IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>
{
    TValue this[ReadOnlySpan<char> key] { get; }

    TValue IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.this[ReadOnlyMemory<char> key] => this[key.Span];

    TValue this[string key] => this[key.AsSpan()];

    bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value);

    bool IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.TryGetValue(ReadOnlyMemory<char> key, [MaybeNullWhen(false)] out TValue value)
        => TryGetValue(key.Span, out value);

    bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        => TryGetValue(key.AsSpan(), out value);

    /// <summary>
    /// Retrieves the value in the longest prefix of the specified query that exists in the trie.
    /// </summary>
    /// <param name="query">a character sequence.</param>
    /// <returns>corresponding value under the key <c>query[..prefixLength]</c>.</returns>
    (int PrefixLength, TValue value) MatchLongestPrefix(ReadOnlySpan<char> query);

    /// <inheritdoc cref="MatchLongestPrefix(ReadOnlySpan{char})"/>
    (int PrefixLength, TValue value) MatchLongestPrefix(ReadOnlyMemory<char> query) => MatchLongestPrefix(query.Span);

    /// <inheritdoc cref="MatchLongestPrefix(ReadOnlySpan{char})"/>
    /// <exception cref="NullReferenceException"><paramref name="query"/> is <c>null</c>.</exception>
    (int PrefixLength, TValue value) MatchLongestPrefix(string query) => MatchLongestPrefix(query.AsSpan());

    IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(ReadOnlySpan<char> prefix);

    IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(ReadOnlyMemory<char> prefix) => EnumEntriesFromPrefix(prefix.Span);

    /// <exception cref="NullReferenceException"><paramref name="prefix"/> is <c>null</c>.</exception>
    IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(string prefix) => EnumEntriesFromPrefix(prefix.AsSpan());
}