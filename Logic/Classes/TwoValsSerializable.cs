using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TranslatorApk.Annotations;

namespace TranslatorApk.Logic.Classes
{
    [Serializable]
    public abstract class TwoValsSerializable<T, K> : INotifyPropertyChanged, IXmlSerializable
    {
        public T Item1
        {
            get => _item1;
            set
            {
                if (_item1?.Equals(value) == true) return;
                _item1 = value;
                OnPropertyChanged(nameof(Item1));
            }
        }
        private T _item1;

        public K Item2
        {
            get => _item2;
            set
            {
                if (_item2.Equals(value)) return;
                _item2 = value;
                OnPropertyChanged(nameof(Item2));
            }
        }
        private K _item2;

        private readonly Func<string, T> _firstFromString;
        private readonly Func<string, K> _secondFromString;
        private readonly Func<T, string> _firstToString;
        private readonly Func<K, string> _secondToString;

        protected TwoValsSerializable() { }

        protected TwoValsSerializable(Func<string, T> firstFromString, Func<string, K> secondFromString, Func<T, string> firstToString, Func<K, string> secondToString)
            : this(default(T), default(K), firstFromString, secondFromString, firstToString, secondToString) { }

        protected TwoValsSerializable(Func<string, T> firstFromString, Func<string, K> secondFromString)
            : this(default(T), default(K), firstFromString, secondFromString) { }

        protected TwoValsSerializable(T item1, K item2, Func<string, T> firstFromString, Func<string, K> secondFromString, Func<T, string> firstToString, Func<K, string> secondToString)
        {
            _firstFromString = firstFromString;
            _secondFromString = secondFromString;
            _firstToString = firstToString;
            _secondToString = secondToString;

            Item1 = item1;
            Item2 = item2;
        }

        protected TwoValsSerializable(T item1, K item2, Func<string, T> firstFromString, Func<string, K> secondFromString) 
            : this(item1, item2, firstFromString, secondFromString, t => t.ToString(), k => k.ToString()) { }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Item1 = _firstFromString(reader["Item1"]);
            Item2 = _secondFromString(reader["Item2"]);
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Item1", _firstToString(Item1));
            writer.WriteAttributeString("Item2", _secondToString(Item2));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
