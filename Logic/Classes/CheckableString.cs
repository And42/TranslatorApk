using System;

namespace TranslatorApk.Logic.Classes
{
    [Serializable]
    public class CheckableString : TwoValsSerializable<string, bool>
    {
        public CheckableString(string text, bool state) : base(text, state, s => s, bool.Parse, s => s, b => b.ToString()) { }

        public CheckableString() : base(s => s, bool.Parse, s => s, b => b.ToString()) { }
    }
}
