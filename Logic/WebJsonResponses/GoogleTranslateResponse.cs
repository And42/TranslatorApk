using System.Collections.Generic;
using System.Text;

// ReSharper disable InconsistentNaming

namespace TranslatorApk.Logic.WebJsonResponses
{
    public class GoogleTranslateResponse
    {
        public List<_sentences> sentences { get; set; }
        public int server_time { get; set; }
        public string src { get; set; }

        public class _sentences
        {
            public string trans { get; set; }
            public string orig { get; set; }
            public string translit { get; set; }
            public string src_translit { get; set; }
        }

        public override string ToString()
        {
            if (sentences == null || sentences.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (_sentences str in sentences)
                sb.Append(str.trans);

            return sb.ToString();
        }
    }
}
