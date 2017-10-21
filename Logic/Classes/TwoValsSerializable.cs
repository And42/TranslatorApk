using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Classes
{
    [Serializable]
    public abstract class TwoValsSerializable<TFirst, TSecond> : IRaisePropertyChanged, IXmlSerializable
    {
        public TFirst Item1
        {
            get => _item1;
            set => this.SetProperty(ref _item1, value);
        }
        private TFirst _item1;

        public TSecond Item2
        {
            get => _item2;
            set => this.SetProperty(ref _item2, value);
        }
        private TSecond _item2;

        private readonly Func<string, TFirst> _firstFromString;
        private readonly Func<string, TSecond> _secondFromString;
        private readonly Func<TFirst, string> _firstToString;
        private readonly Func<TSecond, string> _secondToString;

        protected TwoValsSerializable() { }

        protected TwoValsSerializable(Func<string, TFirst> firstFromString, Func<string, TSecond> secondFromString, Func<TFirst, string> firstToString, Func<TSecond, string> secondToString)
            : this(default, default, firstFromString, secondFromString, firstToString, secondToString) { }

        protected TwoValsSerializable(Func<string, TFirst> firstFromString, Func<string, TSecond> secondFromString)
            : this(default, default, firstFromString, secondFromString) { }

        protected TwoValsSerializable(TFirst item1, TSecond item2, Func<string, TFirst> firstFromString, Func<string, TSecond> secondFromString, Func<TFirst, string> firstToString, Func<TSecond, string> secondToString)
        {
            _firstFromString = firstFromString;
            _secondFromString = secondFromString;
            _firstToString = firstToString;
            _secondToString = secondToString;

            Item1 = item1;
            Item2 = item2;
        }

        protected TwoValsSerializable(TFirst item1, TSecond item2, Func<string, TFirst> firstFromString, Func<string, TSecond> secondFromString) 
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

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
