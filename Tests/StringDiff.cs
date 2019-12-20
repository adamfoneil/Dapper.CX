using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
    /// <summary>
    /// I had to troubleshoot some assert fails in AppVeyor that I couldn't repro locally    
    /// </summary>
    internal static class StringDiff
    {        
        public static void PrintInfo(string string1, string string2)        
        {
            IEnumerable<CharIndex> GetCharIndexes(string input)
            {
                return input.ToCharArray().Select((c, index) => new CharIndex() { Char = c, Index = index });
            };

            if (!string1?.Equals(string2) ?? true)
            {
                var indexes1 = GetCharIndexes(string1);
                var indexes2 = GetCharIndexes(string2);

                var diff = (from x1 in indexes1
                           join x2 in indexes2 on x1.Index equals x2.Index
                           where x1.Char != x2.Char
                           select new
                           {
                               x1.Index,
                               LeftChar = x1.Char,
                               RightChar = x2.Char
                           }).FirstOrDefault();

                if (diff != null)
                {
                    Debug.Print($"position {diff.Index}: char1 = {diff.LeftChar}, char2 = {diff.RightChar}");
                }

                if (string1.Length > string2.Length)
                {
                    Debug.Print($"string1 overhang: {Overhang(string1, string2)}");
                }

                if (string2.Length > string1.Length)
                {
                    Debug.Print($"string2 overhang: {Overhang(string2, string1)}");
                }
            }
            else
            {
                Debug.Print("strings are equal");
            }
        }

        private static string Overhang(string longer, string shorter)
        {
            return longer.Substring(shorter.Length);
        }
    }

    internal struct CharIndex
    {
        public char Char { get; set; }
        public int Index { get; set; }
    }
}
