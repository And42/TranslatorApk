using System;
using System.Collections.Generic;
using System.Windows;
using SettingsManager;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.OrganisationItems
{
    public class AppSettings : SettingsModel
    {
        public virtual string LanguageOfApp { get; set; }

        public virtual Guid OnlineTranslator { get; set; }

        public virtual bool EmptyXml { get; set; }

        public virtual bool EmptySmali { get; set; }

        public virtual bool Images { get; set; }

        public virtual bool FilesWithErrors { get; set; }

        public virtual bool OnlyXml { get; set; }

        public virtual List<string> XmlRules { get; set; }

        public virtual string TargetLanguage { get; set; }

        public virtual bool OnlyFullWords { get; set; }

        public virtual bool MatchCase { get; set; }

        public virtual List<string> EditorSearchAdds { get; set; }

        public virtual List<string> FullSearchAdds { get; set; }

        public virtual bool EditorSOnlyFullWords { get; set; }

        public virtual bool EditorSMatchCase { get; set; }

        public virtual bool MainWMaximized { get; set; }

        public virtual bool EditorWMaximized { get; set; }

        public virtual List<string> OtherExtensions { get; set; }

        public virtual bool OtherFiles { get; set; }

        public virtual List<string> AvailToEditFiles { get; set; }

        public virtual List<string> ImageExtensions { get; set; }

        public virtual Size MainWindowSize { get; set; } = new Size(670, 500);

        public virtual bool OnlyResources { get; set; }

        public virtual string Theme { get; set; }

        public virtual string ApktoolVersion { get; set; }

        public virtual bool ShowPreviews { get; set; } = true;

        public virtual Dictionary<Guid, string> TranslatorServicesKeys { get; set; }

        public virtual bool EmptyFolders { get; set; }

        public virtual bool TopMost { get; set; }

        public virtual string TargetDictionary { get; set; }

        public virtual List<CheckableSetting> SourceDictionaries { get; set; }

        public virtual bool AlternativeEditingKeys { get; set; }

        public virtual bool SessionAutoTranslate { get; set; }

        public virtual bool EditorWindowSaveToDict { get; set; }

        public virtual bool ShowNotifications { get; set; } = true;

        public virtual bool AlternatingRows { get; set; } = true;

        public virtual int TranslationTimeout { get; set; } = 5000;

        public virtual int FontSize { get; set; } = 14;

        public virtual int GridFontSize { get; set; } = 15;

        public virtual bool TVFilterBoxUseRegex { get; set; }

        public virtual bool FixOnlineTranslationResults { get; set; } = true;
    }
}
