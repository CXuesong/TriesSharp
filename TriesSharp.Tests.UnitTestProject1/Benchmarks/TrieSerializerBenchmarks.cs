using BenchmarkDotNet.Attributes;
using TriesSharp.Collections;

namespace TriesSharp.Tests.UnitTestProject1.Benchmarks;

public class TrieSerializerBenchmarks
{

    [Benchmark]
    [ArgumentsSource(nameof(GetSerializeTrieTestArguments))]
    public async ValueTask SerializeTrie(SerializeTrieTestArguments args)
    {
        args.serializedStream.Seek(0, SeekOrigin.Begin);
        await TrieSerializer.Serialize(args.serializedStream, args.trie);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetSerializeTrieTestArguments))]
    public async ValueTask DeserializeTrie(SerializeTrieTestArguments args)
    {
        args.serializedStream.Seek(0, SeekOrigin.Begin);
        await TrieSerializer.Deserialize(args.serializedStream);
    }

    public static IEnumerable<SerializeTrieTestArguments> GetSerializeTrieTestArguments()
    {
        static SerializeTrieTestArguments BuildCase(string fileName)
        {
            var trie = new Trie<ReadOnlyMemory<char>>();
            foreach (var w in TextResourceLoader.LoadWordList(fileName).Distinct())
                trie.Add(w.AsMemory(), w.Reverse().ToArray());
            var ms = new MemoryStream();
            TrieSerializer.Serialize(ms, trie);
            return new(fileName, trie, ms, (int)ms.Length);
        }

        yield return BuildCase(TextResourceLoader.ShijiSnippet1);
        yield return BuildCase(TextResourceLoader.TaleOfTwoCities1);
        yield return BuildCase(TextResourceLoader.WiktionaryTopFreq1000);
        yield return BuildCase(TextResourceLoader.OpenCCSTPhrases);
    }

    public record SerializeTrieTestArguments(string name, Trie<ReadOnlyMemory<char>> trie, MemoryStream serializedStream, int serializedSize)
    {

        /// <inheritdoc />
        public override string ToString() => $"{name} ({trie.Count} items) ({serializedSize:N0} B)";

    }

}
