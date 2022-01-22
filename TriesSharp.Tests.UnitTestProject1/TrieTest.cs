using System;
using System.Collections.Generic;
using System.Linq;
using TriesSharp.Collections;
using Xunit;
using Xunit.Abstractions;

namespace TriesSharp.Tests.UnitTestProject1;

public class TrieTest
{
    public TrieTest(ITestOutputHelper output)
    {
        Output = output;
    }

    protected ITestOutputHelper Output { get; }

    [Theory]
    [InlineData(TextResourceLoader.TaleOfTwoCities1, 10)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1, 50)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1, 100)]
    [InlineData(TextResourceLoader.TaleOfTwoCities1, -1)]
    [InlineData(TextResourceLoader.ShijiSnippet1, 10)]
    [InlineData(TextResourceLoader.ShijiSnippet1, 100)]
    [InlineData(TextResourceLoader.ShijiSnippet1, -1)]
    [InlineData(TextResourceLoader.WiktionaryTopFreq1000, -1)]
    public void DictionaryApiTest(string fileName, int wordCount)
    {
        var baseline = new Dictionary<string, int>();
        var trie = new Trie<int>();
        AssertStateEquality();

        // Corner case: empty key.
        baseline.Add("", -1);
        trie.Add("", -1);
        AssertStateEquality();

        var wordList = TextResourceLoader.LoadWordList(fileName);
        if (wordCount >= 0) wordList = wordList.Take(wordCount).ToList();

        for (int i = 0; i < wordList.Count; i++)
        {
            if (baseline.TryAdd(wordList[i], i))
            {
                Output.WriteLine("Add [{0}, {1}] -> Success", wordList[i], i);
                Assert.False(trie.ContainsKey(wordList[i]));
                trie.Add(wordList[i], i);
                Assert.True(trie.ContainsValue(i));
            }
            else
            {
                Output.WriteLine("Add [{0}, {1}] -> Duplicate", wordList[i], i);
                Assert.True(trie.ContainsKey(wordList[i]));
                Assert.Throws<ArgumentException>(() => trie.Add(wordList[i], i));
                Assert.False(trie.ContainsValue(i));
            }
            AssertStateEquality();
        }

        trie.TrimExcess();

        for (int i = 0; i < wordList.Count; i++)
        {
            if (baseline.Remove(wordList[i]))
            {
                Output.WriteLine("Remove [{0}] -> True", wordList[i]);
                Assert.True(trie.Remove(wordList[i]));
                Assert.False(trie.ContainsKey(wordList[i]));
            }
            else
            {
                Output.WriteLine("Remove [{0}] -> False", wordList[i]);
                Assert.False(trie.Remove(wordList[i]));
            }
            AssertStateEquality();
        }

        void AssertStateEquality()
        {
            Assert.Equal(baseline.Count, trie.Count);
            foreach (var (k, v) in baseline)
            {
                Assert.Equal(v, trie[k]);
                Assert.True(trie.TryGetValue(k, out var v1));
                Assert.Equal(v, v1);
            }
            var iterations = 0;
            foreach (var (k, v) in trie)
            {
                Assert.Equal(baseline[k.ToString()], v);
                iterations++;
            }
            Assert.Equal(baseline.Count, iterations);
        }
    }

    [Fact]
    public void TriePrefixTest()
    {
        var trie = new Trie<int>();
        var wordList = TextResourceLoader.LoadWordList(TextResourceLoader.WiktionaryTopFreq1000);
        for (var i = 0; i < wordList.Count; i++)
        {
            var w = wordList[i];
            trie.Add(w, i);
        }

        // Access all prefix combinations with 2 letters.
        var keyBuffer = new char[2];
        for (var i = 'A'; i < 'z'; i++)
        {
            keyBuffer[0] = i;
            AssertPrefix(keyBuffer.AsMemory(0, 1));
            for (var j = 'A'; j < 'z'; j++)
            {
                keyBuffer[1] = j;
                AssertPrefix(keyBuffer.AsMemory());
            }
        }

        // Corner case: "" can be a valid key!
        Assert.Equal(wordList.Count, trie.EnumEntriesFromPrefix("").Count());
        trie.Add("", -1);
        Assert.Equal(wordList.Count + 1, trie.EnumEntriesFromPrefix("").Count());
        Assert.Contains(trie.EnumEntriesFromPrefix(""), p => p.Key.IsEmpty && p.Value == -1);

        void AssertPrefix(ReadOnlyMemory<char> prefix)
        {
            var expected = wordList.Where(w => w.AsSpan().StartsWith(prefix.Span, StringComparison.Ordinal)).ToHashSet();
            Output.WriteLine("AssertPrefix: {0} ({1})", prefix, expected.Count);
            var actual = trie.EnumEntriesFromPrefix(prefix).Select(p => p.Key.ToString()).ToHashSet();
            Assert.Equal(expected, actual);
        }
    }
}