using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Test.Common.Utilities
{
    /// <summary>
    /// Utility class for creating strings containing random characters of a particular length.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class StringGenerator
    {
        private static readonly Random Random = new();

        private static readonly char[] Chars =
            Enumerable
                .Range('A', 'Z' - 'A' + 1)
                .Select(x => (char)x)
                .Where(c => !char.IsControl(c))
                .ToArray();

        private static char RandomChar() => Chars[Random.Next(0, Chars.Length)];

        /// <summary>
        /// Generates a string with a random set of characters. 
        /// </summary>
        /// <param name="length">The desired string length to be returned</param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++)
            {
                chars[i] = RandomChar();
            }
            return new string(chars);
        }
    }
}
