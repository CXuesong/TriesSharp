using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TriesSharp.Tests.UnitTestProject1
{
    public static class TextResourceLoader
    {

        public const string TaleOfTwoCities1 = "ToTC1.txt";

        public const string ShijiSnippet1 = "Shiji1.txt";

        public static List<string> LoadWordList(string fileName)
        {
            var resourceName = typeof(TextResourceLoader).Namespace + ".TextResource." + fileName;
            using var s = typeof(TextResourceLoader).Assembly.GetManifestResourceStream(resourceName);
            if (s == null) throw new ArgumentException($"Cannot find resource {resourceName} .");
            using var reader = new StreamReader(s);
            var list = new List<string>();
            var sb = new StringBuilder(8);
            int nextChar;
            while ((nextChar = reader.Read()) >= 0)
            {
                var c = (char)nextChar;
                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }
                sb.Append(c);
            }
            if (sb.Length > 0) list.Add(sb.ToString());
            return list;
        }

    }
}
