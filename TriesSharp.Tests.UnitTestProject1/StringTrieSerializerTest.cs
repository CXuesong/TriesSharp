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

public class StringTrieSerializerTest
{

    public StringTrieSerializerTest(ITestOutputHelper output)
    {
        Output = output;
    }

    protected ITestOutputHelper Output { get; }

    private void AssertTrieEquals(Trie<ReadOnlyMemory<char>> trie1, Trie<ReadOnlyMemory<char>> trie2)
    {
        if (trie1 == trie2) return;
        Assert.Equal(trie1.Count, trie2.Count);
        Assert.All(trie1, p => Assert.True(p.Value.Span.Equals(trie2[p.Key].Span, StringComparison.Ordinal)));
        Assert.All(trie2, p => Assert.True(p.Value.Span.Equals(trie1[p.Key].Span, StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(TextResourceLoader.WiktionaryTopFreq1000)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1)]
    public async Task SerializationTest(string fileName)
    {
        var trie = new Trie<ReadOnlyMemory<char>>();
        var wordList = TextResourceLoader.LoadWordList(fileName).Distinct().ToList();
        foreach (var w in wordList)
        {
            if (!trie.ContainsKey(w))
                trie.Add(w.AsMemory(), w.Reverse().ToArray());
        }
        using var ms = new MemoryStream();
        await StringTrieSerializer.Serialize(trie, ms);

        ms.Seek(0, SeekOrigin.Begin);
        var trie1 = await StringTrieSerializer.Deserialize(ms);

        AssertTrieEquals(trie, trie1);
    }

}
