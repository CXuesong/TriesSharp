using System;
using System.Collections.Generic;
using TriesSharp.Collections;
using Xunit;

namespace TriesSharp.Tests.UnitTestProject1;

public class TrieTest
{
    [Fact]
    public void DictionaryApiTest()
    {
        var baseline = new Dictionary<string, int>();
        var trie = new Trie<int>();
        AssertStateEquality();

        var wordList = TextResourceLoader.LoadWordList(TextResourceLoader.TaleOfTwoCities1);

        for (int i = 0; i < wordList.Count; i++)
        {
            if (baseline.TryAdd(wordList[i], i))
            {
                Assert.False(trie.ContainsKey(wordList[i]));
                trie.Add(wordList[i], i);
            }
            else
            {
                Assert.True(trie.ContainsKey(wordList[i]));
                Assert.Throws<ArgumentException>(() => trie.Add(wordList[i], i));
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