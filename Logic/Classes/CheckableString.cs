using System;

namespace TranslatorApk.Logic.Classes
{
    [Serializable]
    public class CheckableString : TwoValsSerializable<string, bool>
    {
        private static T Eq<T>(T val) => val; 

        public CheckableString(string text, bool state) : base(text, state, Eq, bool.Parse, Eq, b => b.ToString()) { }

        public CheckableString() : base(Eq, bool.Parse, Eq, b => b.ToString()) { }
    }
}
