using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriesSharp.Collections;

internal sealed class TrieNode<TValue>
{

    public TValue? Value { get; set; }

    public SortedList<char, TrieNode<TValue>>? Children { get; set; }

}