using System;
using System.Collections.Generic;

namespace TranslatorApk.Logic.Utils
{
    internal static class StringUtils
    {
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static string JoinStr(this IEnumerable<string> collection, string separator)
        {
            return string.Join(separator, collection);
        }

        public static string[] SplitRemove(this string input, params string[] separators)
        {
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitRemove(this string input, params char[] separators)
        {
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitNone(this string input, params string[] separators)
        {
            return input.Split(separators, StringSplitOptions.None);
        }

        public static string[] SplitNone(this string input, params char[] separators)
        {
            return input.Split(separators, StringSplitOptions.None);
        }
    }
}
