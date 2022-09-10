using System.Globalization;

namespace TriesSharp.Tests.UnitTestProject1;

internal static class Utility
{

    public static char[] ReverseString(string str)
    {
        var reversed = new char[str.Length];
        for (int pos = 0, len = 0; pos < str.Length; pos += len)
        {
            len = StringInfo.GetNextTextElementLength(str, pos);
            str.CopyTo(pos, reversed, str.Length - pos - len, len);
        }
        return reversed;
    }

}
