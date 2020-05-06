using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dapper.CX.Static
{
    public static class StringId
    {
        // idea from https://scottlilly.com/create-better-random-numbers-in-c/
        private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

        public static string New(int length, int maxAttempts = -1)
        {
            int attempts = 0;
            return New(length, ref attempts, maxAttempts);
        }

        public static string New(int length, ref int attempts, int maxAttempts = -1)
        {
            var sourceBytes = Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwxyz0123456789");

            string result = null;
            attempts = 0;

            do
            {
                attempts++;

                // the random bytes generated won't necessarily be characters I allow, 
                // so I generate more than I need, and filter for the allowed
                byte[] randomBytes = new byte[length * 10];
                _generator.GetNonZeroBytes(randomBytes);
                var randomAllowedBytes = randomBytes.Where(b => sourceBytes.Contains(b)).Take(length);
                result = new string(randomAllowedBytes.Select(b => (char)b).ToArray());
                if (maxAttempts > 0 && attempts > maxAttempts) throw new Exception($"Couldn't generate desired random characters after {maxAttempts}.");
            } while (result.Length < length);

            // you're not certain to get enough good characters to reach the desired result length,
            // so I have this brute force method of simply trying again until I get the desired length,
            // altough I think you want to be careful with this

            return result;
        }        

    }
}
