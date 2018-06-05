using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            if (collection is HashSet<T> set)
                return set;

            return new HashSet<T>(collection);
        }

        /// <summary>
        /// Добавляет в коллекцию непустой элемент
        /// </summary>
        /// <typeparam name="T">Тип элементов в коллекции</typeparam>
        /// <param name="collection">Коллекция</param>
        /// <param name="item">Элемент</param>
        public static void AddIfNotNull<T>(this ICollection<T> collection, T item) where T : class
        {
            if (item != null)
                collection.Add(item);
        }

        /// <summary>
        /// Select method with error handling
        /// </summary>
        /// <typeparam name="TSource">Source collection type</typeparam>
        /// <typeparam name="TResult">Target collection type</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="selector">Value converter</param>
        /// <param name="onFail">Called when converting an item causes an exception</param>
        public static IEnumerable<TResult> SelectSafe<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector, Action<TSource, Exception> onFail = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            IEnumerable<TResult> _()
            {
                foreach (TSource it in source)
                {
                    TResult res;

                    try
                    {
                        res = selector(it);
                    }
                    catch (Exception ex)
                    {
                        onFail?.Invoke(it, ex);

                        continue;
                    }

                    yield return res;
                }
            }

            return _();
        }

        public static (int index, T value) FindWithIndex<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int index = 0;

            foreach (T item in collection)
            {
                if (predicate(item))
                    return (index, item);

                index++;
            }

            return (-1, default);
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> collection)
        {
            return collection.Select((it, index) => (it, index));
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (T item in collection)
                action(item);
        }

        public static IEnumerable<T> DistinctBy<T, R>(this IEnumerable<T> collection, Func<T, R> selector)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return collection.GroupBy(selector).Select(it => it.First());
        }

        public static R[] SelectArray<T, R>(this T[] collection, Converter<T, R> converter)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            return Array.ConvertAll(collection, converter);
        }
    }
}
