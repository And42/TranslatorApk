using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Utils
{
    public static class TranslationUtils
    {
        private static readonly Regex[] Regexes = {
            new Regex(@"% {1,2}(\d) {1,2}\$ {1,2}(\w)"),
            new Regex(@"% {1,2}(\d|\w)"),
            new Regex(@"% {1,2}(D|д|Д)"),
            new Regex(@"% {1,2}(S|с|С)"),
            new Regex(@"\$ {1,2}(D|д|Д)"), 
            new Regex(@"\$ {1,2}(S|с|С)")
        };

        private static readonly string[] Replacements =
        {
            "%${GROUP1}$${GROUP2}",
            "%${GROUP1}",
            "%d",
            "%s",
            "$d",
            "$s"
        };

        private static readonly Regex GroupRegex = new Regex(@"\$\{GROUP(\d+)\}");

        public static string FixOnlineTranslation(string input)
        {
            foreach (var (regex, index) in Regexes.Enumerate())
                input = ProcessRegex(regex, Replacements[index], input);

            return input;
        }

        private static string ProcessRegex(Regex regex, string replacement, string input)
        {
            StringBuilder builder = null;

            foreach (var match in regex.Matches(input).Cast<Match>().Reverse())
            {
                if (builder == null)
                    builder = new StringBuilder(input);

                builder.Remove(match.Index, match.Length);

                var replacementTemp = replacement;

                foreach (var groupMatch in GroupRegex.Matches(replacement).Cast<Match>().Reverse())
                {
                    int groupIndex = Int32.Parse(groupMatch.Groups[1].Value);

                    replacementTemp = replacementTemp.Remove(groupMatch.Index, groupMatch.Length);
                    replacementTemp = replacementTemp.Insert(groupMatch.Index, match.Groups[groupIndex].Value);
                }

                builder.Insert(match.Index, replacementTemp);
            }

            return builder?.ToString() ?? input;
        }

        /// <summary>
        /// Переводит указанный текст с помощью онлайн переводчика, основываясь на текущих настройках
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        public static string TranslateTextWithSettings(string text)
        {
            var translated = GlobalVariables.CurrentTranslationService.Translate(text, SettingsIncapsuler.Instance.TargetLanguage);

            if (SettingsIncapsuler.Instance.FixOnlineTranslationResults)
                return FixOnlineTranslation(translated);

            return translated;
        }
    }
}
