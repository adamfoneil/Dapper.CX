using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.CX.Static
{
    public static class StringId
    {
        public static string New(int length)
        {
            var sourceChars = "abcdefghijklmnopqrstuvwxyz01234567890.-".ToCharArray();

            int nonConsecutiveFromRight = 2;
            HashSet<int> nonConsecutiveIndexes = new HashSet<int>();
            for (int i = sourceChars.Length - nonConsecutiveFromRight; i < sourceChars.Length; i++) nonConsecutiveIndexes.Add(i);

            StringBuilder sb = new StringBuilder();
                        
            var rnd = new Random();
            int index = 0;
            int priorIndex = 0;
            var maximums = Enumerable.Range(1, 6).Select(i => i * 8).ToArray();
            while (sb.Length < length)
            {                
                int max = maximums[rnd.Next(maximums.Length)];
                int increment = rnd.Next(max);
                int forwardOrBackward = ((rnd.Next(max) % 2) == 0) ? 1 : -1;
                index += increment * forwardOrBackward;
                if (index < 0) index *= -1;
                if (index > sourceChars.Length - 1) index %= sourceChars.Length;                

                if (nonConsecutiveIndexes.Contains(index))
                {
                    while (index == priorIndex)
                    {
                        increment = rnd.Next(max);                        
                        forwardOrBackward = ((rnd.Next(max) % 2) == 0) ? 1 : -1;
                        index += increment * forwardOrBackward;
                        if (index < 0) index *= -1;
                        if (index > sourceChars.Length - 1) index %= sourceChars.Length;
                    }
                }
                
                sb.Append(sourceChars[index]);
                priorIndex = index;
            }

            return sb.ToString();
        }
    }
}
