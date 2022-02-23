using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TriesSharp.Collections;

namespace TriesSharp.Tests.UnitTestProject1.Benchmarks;

public class TrieBenchmarks
{

    [Benchmark]
    [ArgumentsSource(nameof(GetLoadTrieTestArguments))]
    public void LoadTrie(List<string> wordList)
    {
        var trie = new Trie<int>();
        for (int i = 0; i < wordList.Count; i++)
            trie[wordList[i]] = i;
    }

    public static IEnumerable<List<string>> GetLoadTrieTestArguments()
    {
        yield return TextResourceLoader.LoadWordList(TextResourceLoader.ShijiSnippet1);
        yield return TextResourceLoader.LoadWordList(TextResourceLoader.TaleOfTwoCities1);
        yield return TextResourceLoader.LoadWordList(TextResourceLoader.WiktionaryTopFreq1000);
    }

}
