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
    public void DictionaryApiTest(string fileName, int wordCount)
    {
        var baseline = new Dictionary<string, int>();
        var trie = new Trie<int>();
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
}