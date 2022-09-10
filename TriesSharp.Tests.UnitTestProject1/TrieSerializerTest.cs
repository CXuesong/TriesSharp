using System.IO.Compression;
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
    [InlineData(TextResourceLoader.WiktionaryTopFreq1000, false)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1, false)]
    [InlineData(TextResourceLoader.ShijiSnippet1, false)]
    [InlineData(TextResourceLoader.OpenCCSTPhrases, false)]
    [InlineData(TextResourceLoader.WiktionaryTopFreq1000, true)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1, true)]
    [InlineData(TextResourceLoader.ShijiSnippet1, true)]
    [InlineData(TextResourceLoader.OpenCCSTPhrases, true)]
    public async Task SerializationTest(string fileName, bool gzipStream)
    {
        var trie = new Trie<ReadOnlyMemory<char>>();
        foreach (var w in TextResourceLoader.LoadWordList(fileName).Distinct())
            trie.Add(w.AsMemory(), Utility.ReverseString(w));
        using var ms = new MemoryStream();
        if (gzipStream)
        {
            // By using GZipStream, we are disabling Stream.CanSeek.
            await using var gs = new GZipStream(ms, CompressionLevel.Fastest, true);
            Assert.False(gs.CanSeek);
            await TrieSerializer.Serialize(gs, trie);
        }
        else
        {
            await TrieSerializer.Serialize(ms, trie);
        }

        Output.WriteLine("Serialized size: {0:N0}", ms.Length);
        ms.Seek(0, SeekOrigin.Begin);

        Trie<ReadOnlyMemory<char>> trie1;
        if (gzipStream)
        {
            // By using BufferedStream, we are disabling Stream.CanSeek.
            await using var gs = new GZipStream(ms, CompressionMode.Decompress, true);
            Assert.False(gs.CanSeek);
            trie1 = await TrieSerializer.Deserialize(gs);
        }
        else
        {
            trie1 = await TrieSerializer.Deserialize(ms);
        }

        AssertTrieEquals(trie, trie1);
    }

}
