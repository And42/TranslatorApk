using System.Collections;
using System.Collections.Generic;

namespace TranslatorApk.Logic.Classes
{
    public class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

        public Map()
        {
            Forward = new Indexer<T1, T2>(_forward);
            Reverse = new Indexer<T2, T1>(_reverse);
        }

        public class Indexer<T3, T4>
        {
            private readonly Dictionary<T3, T4> _dictionary;

            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }

            public T4 this[T3 index]
            {
                get { return _dictionary[index]; }
                set { _dictionary[index] = value; }
            }
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public Indexer<T1, T2> Forward { get; private set; }
        public Indexer<T2, T1> Reverse { get; private set; }

        public bool ContainsKey(T1 key) => _forward.ContainsKey(key);

        public bool ContainsValue(T2 value) => _reverse.ContainsKey(value);

        public void RemoveKey(T1 key)
        {
            T2 value;
            if (_forward.TryGetValue(key, out value))
            {
                _forward.Remove(key);
                _reverse.Remove(value);
            }
        }

        public void RemoveValue(T2 value)
        {
            T1 key;
            if (_reverse.TryGetValue(value, out key))
            {
                _reverse.Remove(value);
                _forward.Remove(key);
            }
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return _forward.TryGetValue(key, out value);
        }

        public bool TryGetKey(T2 value, out T1 key)
        {
            return _reverse.TryGetValue(value, out key);
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return _forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
