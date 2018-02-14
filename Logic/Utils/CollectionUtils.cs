using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslatorApk.Logic.Utils
{
    internal static class CollectionUtils
    {
        public static bool In<T>(this T item, params T[] collection)
        {
            return collection.Contains(item);
        }

        public static bool In<T>(this T item, IEnumerable<T> collection)
        {
            return collection.Contains(item);
        }

        public static bool Contains<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value) != -1;
        }
    }
}
