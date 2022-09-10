using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriesSharp.Collections;
using Xunit;
using Xunit.Abstractions;

namespace TriesSharp.Tests.UnitTestProject1;

public class TrieSerializerTest
{

    public TrieSerializerTest(ITestOutputHelper output)
    {
        Output = output;
    }

    protected ITestOutputHelper Output { get; }

    private void AssertTrieEquals(Trie<ReadOnlyMemory<char>> expectTrie, Trie<ReadOnlyMemory<char>> actualTrie)
    {
        if (expectTrie == actualTrie) return;
        Assert.Equal(expectTrie.Count, actualTrie.Count);
        Assert.All(expectTrie, p =>
        {
            if (!p.Value.Span.Equals(actualTrie[p.Key].Span, StringComparison.Ordinal))
                Assert.Fail($"Trie key: {p.Key} expect: {p.Value} actual: {actualTrie[p.Key]}");
        });
        Assert.All(actualTrie, p => Assert.True(p.Value.Span.Equals(expectTrie[p.Key].Span, StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(TextResourceLoader.WiktionaryTopFreq1000)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1)]
    [InlineData(TextResourceLoader.ShijiSnippet1)]
    [InlineData(TextResourceLoader.OpenCCSTPhrases)]
    public async Task SerializationTest(string fileName)
    {
        var trie = new Trie<ReadOnlyMemory<char>>();
        foreach (var w in TextResourceLoader.LoadWordList(fileName).Distinct())
            trie.Add(w.AsMemory(), Utility.ReverseString(w));
        using var ms = new MemoryStream();
        await TrieSerializer.Serialize(ms, trie);

        ms.Seek(0, SeekOrigin.Begin);
        var trie1 = await TrieSerializer.Deserialize(ms);

        AssertTrieEquals(trie, trie1);
    }

}
