using System;
using System.Collections.Generic;

namespace TranslatorApk.Logic.Classes
{
    public class ComparisonWrapper<T> : IComparer<T>, IEqualityComparer<T>
    {
        private readonly Comparison<T> _comparison;
        private readonly Func<T, int> _provideHash;

        public ComparisonWrapper(Comparison<T> comparison, Func<T, int> getHashCode = null)
        {
            _comparison = comparison;
            _provideHash = getHashCode ?? (it => it.GetHashCode());
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }

        public bool Equals(T x, T y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(T obj)
        {
            return _provideHash(obj);
        }
    }
}
