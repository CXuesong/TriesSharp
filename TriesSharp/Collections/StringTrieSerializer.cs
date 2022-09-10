using System.Buffers;
using System.Text;

namespace TriesSharp.Collections;

/// <summary>
/// Represents a memory-efficient serialization and deserialization support for <see cref="Trie{TValue}"/> with <c>ReadOnlyMemory&lt;char&gt;</c> as values.
/// </summary>
public static class StringTrieSerializer
{

    private const uint streamMagicHeader = 0x54726948;
    private const uint serializationVersionHeader = 0x000001;

    public static ValueTask Serialize(Trie<ReadOnlyMemory<char>> trie, Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(streamMagicHeader);
        writer.Write(serializationVersionHeader);

        void SerializeNode(TrieNode<ReadOnlyMemory<char>> node)
        {
            // 0: Value
            if (node.HasValue)
            {
                writer.Write7BitEncodedInt(node.Value.Length + 1);
                writer.Write(node.Value.Span);
            }
            else
            {
                writer.Write7BitEncodedInt(0);
            }
            // Persist in pre-order
            // 1: ChildrenCount
            writer.Write7BitEncodedInt(node.ChildrenCount);
            // 2..(2+n): Children keys
            foreach (var child in node.children) writer.Write(child.Key);
            // (2+n)..(2+2n): Node content
            foreach (var child in node.children) SerializeNode(child.Value);
        }

        SerializeNode(trie.GetRootNode());
        return ValueTask.CompletedTask;
    }

    public static ValueTask<Trie<ReadOnlyMemory<char>>> Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        var valuePoolWriter = new ArrayBufferWriter<char>(256);
        var initialValuePoolRef = valuePoolWriter.WrittenMemory[..0];
        var valueLengths = new Queue<int>();
        var valueCount = 0;
        var maxValueLength = 0;

        var intValue = reader.ReadUInt32();
        if (intValue != streamMagicHeader) throw new InvalidDataException("Invalid magic header in key stream.");
        intValue = reader.ReadUInt32();
        if (intValue != serializationVersionHeader) throw new InvalidDataException("Invalid serialization version in key stream.");

        void DeserializeNode(TrieNode<ReadOnlyMemory<char>> node)
        {
            // 0: Value
            var valueLength = reader.Read7BitEncodedInt() - 1;
            if (valueLength > 0)
            {
                // Has value
                valueLengths.Enqueue(valueLength);
                valueCount++;
                maxValueLength = Math.Max(maxValueLength, valueLength);
                var memory = valuePoolWriter.GetMemory(valueLength)[..valueLength];
                if (reader.Read(memory.Span) < valueLength)
                    throw new EndOfStreamException();
                node.SetValue(memory);
                valuePoolWriter.Advance(valueLength);
            }
            else if (valueLength == 0)
            {
                // Has value of empty string
                node.SetValue(ReadOnlyMemory<char>.Empty);
            } else {
                // No value
                node.UnsetValue();
            }
            // Persist in pre-order
            // 1: ChildrenCount
            var childrenCount = reader.Read7BitEncodedInt();
            node.children.Capacity = childrenCount;
            // 2..(2+n): Children keys
            for (int i = 0; i < childrenCount; i++)
                node.children.GetOrAddDefault(reader.ReadChar());
            // (2+n)..(2+2n): Node content
            for (int i = 0; i < childrenCount; i++)
                DeserializeNode(node.children.GetValueAt(i));
        }

        var currentValuePoolOffset = 0;
        void FixNodeValueMemoryRef(TrieNode<ReadOnlyMemory<char>> node)
        {
            if (node.HasValue)
            {
                var length = valueLengths.Dequeue();
                node.SetValue(valuePoolWriter.WrittenMemory[currentValuePoolOffset..(currentValuePoolOffset + length)]);
                currentValuePoolOffset += length;
            }
            var count = node.ChildrenCount;
            for (int i = 0; i < count; i++)
                FixNodeValueMemoryRef(node.children.GetValueAt(i));
        }

        var root = new TrieNode<ReadOnlyMemory<char>>();
        DeserializeNode(root);
        // Fix Memory references if ArrayBufferWriter has been reallocated.
        if (!valuePoolWriter.WrittenMemory[..0].Equals(initialValuePoolRef))
        {
            FixNodeValueMemoryRef(root);
        }
        return ValueTask.FromResult(new Trie<ReadOnlyMemory<char>>(root, valueCount, maxValueLength));
    }

}
