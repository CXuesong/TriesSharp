using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriesSharp.Collections
{
    public abstract class Trie<TValue> : ITrie<TValue>
    {

        /// <inheritdoc />
        public abstract IEnumerator<KeyValuePair<ReadOnlyMemory<char>, TValue>> GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public abstract void Add(KeyValuePair<ReadOnlyMemory<char>, TValue> item);

        /// <inheritdoc />
        public abstract void Clear();

        /// <inheritdoc />
        bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.Contains(KeyValuePair<ReadOnlyMemory<char>, TValue> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract void CopyTo(KeyValuePair<ReadOnlyMemory<char>, TValue>[] array, int arrayIndex);

        /// <inheritdoc />
        public abstract bool Remove(KeyValuePair<ReadOnlyMemory<char>, TValue> item);

        /// <inheritdoc cref="ICollection{T}.Count" />
        public abstract int Count { get; }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<ReadOnlyMemory<char>, TValue>>.IsReadOnly => false;

        /// <inheritdoc />
        bool IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.ContainsKey(ReadOnlyMemory<char> key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract bool Remove(ReadOnlyMemory<char> key);

        /// <inheritdoc />
        bool IDictionary<ReadOnlyMemory<char>, TValue>.ContainsKey(ReadOnlyMemory<char> key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerable<ReadOnlyMemory<char>> IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.Keys => Keys;

        /// <inheritdoc />
        public ICollection<TValue> Values { get; } = new ValueCollection();

        /// <inheritdoc />
        public ICollection<ReadOnlyMemory<char>> Keys { get; } = new KeyCollection();

        /// <inheritdoc />
        IEnumerable<TValue> IReadOnlyDictionary<ReadOnlyMemory<char>, TValue>.Values => Values;

        /// <inheritdoc cref="ITrie{TValue}.this[ReadOnlySpan{char}]" />
        public abstract TValue this[ReadOnlySpan<char> key] { get; set; }

        /// <inheritdoc />
        public abstract bool TryGetValue(ReadOnlySpan<char> key, out TValue value);

        /// <inheritdoc />
        public abstract (int PrefixLength, TValue value) MatchLongestPrefix(ReadOnlySpan<char> query);

        /// <inheritdoc />
        public abstract IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> EnumEntriesFromPrefix(ReadOnlySpan<char> prefix);

        /// <inheritdoc />
        public abstract void Add(ReadOnlySpan<char> key, TValue value);

        private sealed class KeyCollection : ICollection<ReadOnlyMemory<char>>
        {
            /// <inheritdoc />
            public IEnumerator<ReadOnlyMemory<char>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc />
            public void Add(ReadOnlyMemory<char> item)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void Clear()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public bool Contains(ReadOnlyMemory<char> item)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void CopyTo(ReadOnlyMemory<char>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public bool Remove(ReadOnlyMemory<char> item)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public int Count { get; }

            /// <inheritdoc />
            public bool IsReadOnly { get; }
        }

        private sealed class ValueCollection : ICollection<TValue>
        {
            /// <inheritdoc />
            public IEnumerator<TValue> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc />
            public void Add(TValue item)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void Clear()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public bool Contains(TValue item)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public bool Remove(TValue item)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public int Count { get; }

            /// <inheritdoc />
            public bool IsReadOnly { get; }
        }
    }
}
