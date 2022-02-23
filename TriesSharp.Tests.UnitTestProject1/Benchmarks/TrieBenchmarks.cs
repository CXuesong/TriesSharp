using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using TriesSharp.Collections;

namespace TriesSharp.Tests.UnitTestProject1.Benchmarks;

public class TrieBenchmarks
{

    [Benchmark]
    [ArgumentsSource(nameof(GetLoadTrieTestArguments))]
    public void LoadTrie(LoadTrieTestArguments args)
    {
        var trie = new Trie<int>();
        var list = args.wordList;
        for (int i = 0; i < list.Count; i++)
            trie.Add(list[i], i);
    }

    public static IEnumerable<LoadTrieTestArguments> GetLoadTrieTestArguments()
    {
        static LoadTrieTestArguments BuildCase(string fileName)
            => new(fileName, TextResourceLoader.LoadWordList(fileName).Distinct().ToList());

        yield return BuildCase(TextResourceLoader.ShijiSnippet1);
        yield return BuildCase(TextResourceLoader.TaleOfTwoCities1);
        yield return BuildCase(TextResourceLoader.WiktionaryTopFreq1000);
        yield return BuildCase(TextResourceLoader.OpenCCSTPhrases);
    }

    public record LoadTrieTestArguments(string name, List<string> wordList)
    {
        /// <inheritdoc />
        public override string ToString() => $"{name} ({wordList.Count} words)";
    }
    
}
