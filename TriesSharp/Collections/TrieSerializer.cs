using System.Buffers;
using System.Text;

namespace TriesSharp.Collections;

/// <summary>
/// Represents a memory-efficient serialization and deserialization support for <see cref="Trie{TValue}"/> with certain types of values.
/// </summary>
/// <remarks>
/// For now, only values of <c>ReadOnlyMemory&lt;char&gt;</c> is supported.
/// </remarks>
public static class TrieSerializer
{

    private const uint streamMagicHeader = 0x54726948;
    private const uint serializationVersionHeader = 0x000001;

    public static ValueTask Serialize(Stream stream, Trie<ReadOnlyMemory<char>> trie)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        // 0: Header magic
        writer.Write(streamMagicHeader);
        // 1: Serialization version
        writer.Write(serializationVersionHeader);
        // 2: Reserved
        writer.Write(0U);
        writer.Flush();
        // 3: value pool size hint
        var valuePoolSizeHintPos = stream.CanSeek ? stream.Position : -1;
        var valuePoolSizeHint = 0;
        // value pool size hint (we will try to rewind and fill this blank).
        writer.Write(valuePoolSizeHint);
        // 4: Reserved
        writer.Write(0U);

        void SerializeNode(TrieNode<ReadOnlyMemory<char>> node)
        {
            // 0: Value
            if (node.HasValue)
            {
                writer.Write7BitEncodedInt(node.Value.Length + 1);
                valuePoolSizeHint += node.Value.Length;
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
            foreach (var child in node.children) writer.Write7BitEncodedInt(child.Key);
            // (2+n)..(2+2n): Node content
            foreach (var child in node.children) SerializeNode(child.Value);
        }

        SerializeNode(trie.GetRootNode());
        writer.Flush();

        // Fill in the blank.
        if (stream.CanSeek && valuePoolSizeHintPos >= 0)
        {
            var lastPos = stream.Position;
            stream.Position = valuePoolSizeHintPos;
            writer.Write(valuePoolSizeHint);
            writer.Flush();
            stream.Position = lastPos;
        }
        return ValueTask.CompletedTask;
    }

    public static ValueTask<Trie<ReadOnlyMemory<char>>> Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        // 0: Header magic
        var intValue = reader.ReadUInt32();
        if (intValue != streamMagicHeader) throw new InvalidDataException("Invalid magic header in key stream.");
        // 1: Serialization version
        intValue = reader.ReadUInt32();
        if (intValue != serializationVersionHeader) throw new InvalidDataException("Invalid serialization version in key stream.");
        // 2: Reserved
        reader.ReadUInt32();
        // 3: value pool size hint
        var valuePoolSizeHint = reader.ReadInt32();
        // 4: Reserved
        reader.ReadUInt32();

        var valuePoolWriter = new ArrayBufferWriter<char>(valuePoolSizeHint > 0 ? valuePoolSizeHint : 256);
        var initialValuePoolRef = valuePoolWriter.WrittenMemory[..0];
        var valueLengths = valuePoolSizeHint > 0 ? null : new Queue<int>();
        var valueCount = 0;
        var maxValueLength = 0;

        void DeserializeNode(TrieNode<ReadOnlyMemory<char>> node)
        {
            // 0: Value
            var valueLength = reader.Read7BitEncodedInt() - 1;
            if (valueLength > 0)
            {
                // Has value
                valueLengths?.Enqueue(valueLength);
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
                node.children.GetOrAddDefault((char)reader.Read7BitEncodedInt());
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
            if (valueLengths == null)
                throw new InvalidDataException($"ValuePoolSizeHint value mismatch. Expect {valuePoolWriter.WrittenCount}; actual: {valuePoolSizeHint}.");
            FixNodeValueMemoryRef(root);
        }
        return ValueTask.FromResult(new Trie<ReadOnlyMemory<char>>(root, valueCount, maxValueLength));
    }

}
