using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Resources.Files;
using TranslatorApk.Resources.Localizations;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.OrganisationItems
{
    internal class LanguageCodesHelper : BindableBase
    {
        [DebuggerDisplay("{" + nameof(LanguageIso) + "} -> {" +  nameof(CountryIso) + "}")]
        private class IsoPair
        {
            public string LanguageIso { get; }
            public string CountryIso { get; }

            public IsoPair(string languageIso, string countryIso)
            {
                LanguageIso = languageIso;
                CountryIso = countryIso;
            }
        }

        public static LanguageCodesHelper Instanse { get; } = new LanguageCodesHelper();

        public ReadOnlyCollection<string> IsoLanguages { get; }

        public ReadOnlyCollection<string> IsoLongLanguages
        {
            get => _isoLongLanguages;
            private set => SetProperty(ref _isoLongLanguages, value);
        }
        private ReadOnlyCollection<string> _isoLongLanguages;

        private ReadOnlyCollection<IsoPair> IsoList { get; }
        private Dictionary<string, string> LanguageIsoToCountryIsoDict { get; }

        private LanguageCodesHelper()
        {
            IsoLanguages = new ReadOnlyCollection<string>(Properties.Resources.ISOLanguages.Split('|'));

            IsoList =
                new ReadOnlyCollection<IsoPair>(
                    FileResources
                        .language_iso_to_country_iso
                        .SplitFN(Environment.NewLine)
                        .Select(line =>
                        {
                            string[] split = line.SplitFN("->");
                            return new IsoPair(split[0], split[1]);
                        })
                        .ToList()
                );

            for (int i = 0; i < IsoList.Count - 1; i++)
            {
                string lang = IsoList[i].LanguageIso;

                for (int j = i + 1; j < IsoList.Count; j++)
                {
                    string nl = IsoList[j].LanguageIso;
                    if (lang == nl)
                        Debug.WriteLine($"lang: {lang}, index: {i + 1} -> {j + 1}", "Error");
                }
            }

            LanguageIsoToCountryIsoDict = IsoList.ToDictionary(it => it.LanguageIso, it => it.CountryIso);

            Reload();
        }

        public void Reload()
        {
            IsoLongLanguages = new ReadOnlyCollection<string>(StringResources.ISOLongLanguages.Split('|'));

            Debug.Assert(IsoLanguages.Count == IsoLongLanguages.Count, "IsoLanguages.Count == IsoLongLanguages.Count");
        }

        public string GetLanguageByLanguageIso(string languageIso)
        {
            if (string.IsNullOrEmpty(languageIso))
                return null;

            int index = IsoLanguages.IndexOf(languageIso);
            return index == -1 ? null : IsoLongLanguages[index];
        }

        public string GetLanguageIsoByLanguage(string language)
        {
            if (string.IsNullOrEmpty(language))
                return null;

            int index = IsoLongLanguages.IndexOf(language);
            return index == -1 ? null : IsoLanguages[index];
        }

        public string GetCountryIsoByLanguage(string language)
        {
            if (string.IsNullOrEmpty(language))
                return null;

            string languageIso = GetLanguageIsoByLanguage(language);
            return GetCountryIsoByLanguageIso(languageIso);
        }

        public string GetCountryIsoByLanguageIso(string languageIso)
        {
            if (string.IsNullOrEmpty(languageIso))
                return null;

            if (LanguageIsoToCountryIsoDict.TryGetValue(languageIso, out var coutryIso))
                return coutryIso;

            return null;
        }

        public string GetLangNameForFolder(string folderName)
        {
            var nameSplit = folderName.Split('-');

            if (nameSplit.Length <= 1 || nameSplit[0] != "values")
                return null;

            var languageIso = nameSplit[1];
            var countryIso = GetCountryIsoByLanguageIso(languageIso);

            if (string.IsNullOrEmpty(countryIso))
                return null;

            // folder is a language folder
            string language = GetLanguageByLanguageIso(languageIso);

            string source = folderName;
            string found = "values-" + languageIso;

            folderName = language;

            if (found != source)
                folderName += $" ({source.Substring(found.Length).TrimStart('-').TrimStart('r')})";

            return folderName;
        }
    }
}
