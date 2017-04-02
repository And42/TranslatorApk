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
            get { return _item1; }
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
            get { return _item2; }
            set
            {
                if (_item2.Equals(value)) return;
                _item2 = value;
                OnPropertyChanged(nameof(Item2));
            }
        }
        private K _item2;

        private readonly Func<string, T> firstFromString;
        private readonly Func<string, K> secondFromString;
        private readonly Func<T, string> firstToString;
        private readonly Func<K, string> secondToString;

        protected TwoValsSerializable() { }

        protected TwoValsSerializable(Func<string, T> firstFromString, Func<string, K> secondFromString, Func<T, string> firstToString, Func<K, string> secondToString)
            : this(default(T), default(K), firstFromString, secondFromString, firstToString, secondToString) { }

        protected TwoValsSerializable(Func<string, T> firstFromString, Func<string, K> secondFromString)
            : this(default(T), default(K), firstFromString, secondFromString) { }

        protected TwoValsSerializable(T item1, K item2, Func<string, T> firstFromString, Func<string, K> secondFromString, Func<T, string> firstToString, Func<K, string> secondToString)
        {
            this.firstFromString = firstFromString;
            this.secondFromString = secondFromString;
            this.firstToString = firstToString;
            this.secondToString = secondToString;

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
            Item1 = firstFromString(reader["Item1"]);
            Item2 = secondFromString(reader["Item2"]);
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Item1", firstToString(Item1));
            writer.WriteAttributeString("Item2", secondToString(Item2));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
