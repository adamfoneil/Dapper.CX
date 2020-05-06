using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Dapper.CX.Static
{
    public static class StringId
    {
        public static string New(int length)
        {
            var rnd = new Random();

            var sourceChars = "abcdefghijklmnopqrstuvwxyz01234567890"
                .ToCharArray()
                .Select(c => new { Character = c, SortOrder = rnd.Next(1000) })
                .OrderBy(item => item.SortOrder)
                .Select(item => item.Character)
                .ToArray();
            
            StringBuilder sb = new StringBuilder();
                                    
            int index = 0;
            const int max = 100;
            while (sb.Length < length)
            {                                
                int increment = rnd.Next(max);                
                index += increment;                
                if (index > sourceChars.Length - 1) index %= sourceChars.Length;                                
                sb.Append(sourceChars[index]);                
            }

            return sb.ToString();
        }
    }
}
