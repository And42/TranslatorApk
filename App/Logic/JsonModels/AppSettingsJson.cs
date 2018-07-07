using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.JsonModels
{
    internal class AppSettingsJson
    {
        [JsonProperty("language_of_app")]
        public string LanguageOfApp { get; set; }

        [JsonProperty("online_translator")]
        public Guid OnlineTranslator { get; set; }

        [JsonProperty("editor_font_size")]
        public int EditorFontSize { get; set; }

        [JsonProperty("row_height")]
        public int RowHeight { get; set; } = 120;

        [JsonProperty("open_last_folder")]
        public bool OpenLastFolder { get; set; }

        [JsonProperty("empty_xml")]
        public bool EmptyXml { get; set; }

        [JsonProperty("empty_smali")]
        public bool EmptySmali { get; set; }

        [JsonProperty("images")]
        public bool Images { get; set; }

        [JsonProperty("files_with_errors")]
        public bool FilesWithErrors { get; set; }

        [JsonProperty("system_app")]
        public bool SystemApp { get; set; }

        [JsonProperty("only_xml")]
        public bool OnlyXml { get; set; }

        [JsonProperty("xml_rules")]
        public List<string> XmlRules { get; set; }

        [JsonProperty("current_culture")]
        public string CurrentCulture { get; set; }

        [JsonProperty("target_language")]
        public string TargetLanguage { get; set; }

        [JsonProperty("only_full_words")]
        public bool OnlyFullWords { get; set; }

        [JsonProperty("match_case")]
        public bool MatchCase { get; set; }

        [JsonProperty("editor_search_adds")]
        public List<string> EditorSearchAdds { get; set; }

        [JsonProperty("full_search_adds")]
        public List<string> FullSearchAdds { get; set; }

        [JsonProperty("editor_s_only_full_words")]
        public bool EditorSOnlyFullWords { get; set; }

        [JsonProperty("editor_s_match_case")]
        public bool EditorSMatchCase { get; set; }

        [JsonProperty("main_w_maximized")]
        public bool MainWMaximized { get; set; }

        [JsonProperty("editor_w_maximized")]
        public bool EditorWMaximized { get; set; }

        [JsonProperty("editor_w_height")]
        public int EditorWHeight { get; set; }

        [JsonProperty("editor_w_width")]
        public int EditorWWidth { get; set; }

        [JsonProperty("other_extensions")]
        public List<string> OtherExtensions { get; set; }

        [JsonProperty("other_files")]
        public bool OtherFiles { get; set; }

        [JsonProperty("avail_to_edit_files")]
        public List<string> AvailToEditFiles { get; set; }

        [JsonProperty("image_extensions")]
        public List<string> ImageExtensions { get; set; }

        [JsonProperty("replace_sequences")]
        public bool ReplaceSequences { get; set; } = true;

        [JsonProperty("main_window_size")]
        public Size MainWindowSize { get; set; } = new Size(670, 500);

        [JsonProperty("only_resources")]
        public bool OnlyResources { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("apktool_version")]
        public string ApktoolVersion { get; set; }

        [JsonProperty("show_previews")]
        public bool ShowPreviews { get; set; } = true;

        [JsonProperty("translator_services_keys")]
        public Dictionary<string, string> TranslatorServicesKeys { get; set; }

        [JsonProperty("empty_folders")]
        public bool EmptyFolders { get; set; }

        [JsonProperty("top_most")]
        public bool TopMost { get; set; }

        [JsonProperty("target_dictionary")]
        public string TargetDictionary { get; set; }

        [JsonProperty("source_dictionaries")]
        public List<CheckableString> SourceDictionaries { get; set; }

        [JsonProperty("alternative_editing_keys")]
        public bool AlternativeEditingKeys { get; set; }

        [JsonProperty("session_auto_translate")]
        public bool SessionAutoTranslate { get; set; }

        [JsonProperty("editor_window_save_to_dict")]
        public bool EditorWindowSaveToDict { get; set; }

        [JsonProperty("show_notifications")]
        public bool ShowNotifications { get; set; } = true;

        [JsonProperty("alternating_rows")]
        public bool AlternatingRows { get; set; } = true;

        [JsonProperty("translation_timeout")]
        public int TranslationTimeout { get; set; } = 5000;

        [JsonProperty("font_size")]
        public int FontSize { get; set; } = 14;

        [JsonProperty("grid_font_size")]
        public int GridFontSize { get; set; } = 15;

        [JsonProperty("t_v_filter_box_use_regex")]
        public bool TVFilterBoxUseRegex { get; set; }

        [JsonProperty("fix_online_translation_results")]
        public bool FixOnlineTranslationResults { get; set; } = true;
    }
}
