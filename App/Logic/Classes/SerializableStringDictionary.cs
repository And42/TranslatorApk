using System;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TranslatorApk.Logic.Classes
{
    public class SerializableStringDictionary : StringDictionary, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            string typeName = GetType().Name;

            while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == typeName))
            {
                string name = reader["Name"];

                if (name == null)
                    throw new FormatException();

                string value = reader["Value"];
                this[name] = value;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (string key in Keys)
            {
                writer.WriteStartElement("Pair");
                writer.WriteAttributeString("Name", key);
                writer.WriteAttributeString("Value", this[key]);
                writer.WriteEndElement();
            }
        }
    }
}
