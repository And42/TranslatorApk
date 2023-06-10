using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Windows;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Windows;
using TranslatorApkPluginLib;

namespace TranslatorApk.Logic.Utils
{
    internal static class PluginUtils
    {
        private static readonly GlobalVariables GlobalVariables = GlobalVariables.Instance;

        private static bool _pluginsLoaded;

        /// <summary>
        /// Загружает плагин
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void LoadPlugin(string path)
        {
            var assemblyContext = new AssemblyLoadContext(name: Path.GetFileNameWithoutExtension(path), isCollectible: true);
            var loader = new PluginHost(assemblyContext);

            loader.Load(path);

            Application.Current?.Dispatcher.InvokeAction(() =>
            {
                loader.Actions.ForEach(it => AddAction(loader, it));
                loader.Translators.ForEach(AddTranslator);
            });

            GlobalVariables.Plugins.Add(loader.Name, loader);
        }

        /// <summary>
        /// Выгружает плагин
        /// </summary>
        /// <param name="name">Название плагина</param>
        public static void UnloadPlugin(string name)
        {
            PluginHost found;

            if (!GlobalVariables.Plugins.TryGetValue(name, out found))
                return;

            Application.Current.Dispatcher.InvokeAction(() =>
            {
                found.Actions.ForEach(RemoveAction);
                found.Translators.ForEach(RemoveTranslator);
            });

            string pluginName = found.Name;

            found.LoadContext.Unload();

            GlobalVariables.Plugins.Remove(pluginName);
        }

        /// <summary>
        /// Добавляет действие в меню
        /// </summary>
        /// <param name="host"></param>
        /// <param name="action">Действие</param>
        public static void AddAction(PluginHost host, IAdditionalAction action)
        {
            WindowManager.GetActiveWindow<MainWindow>()?.ViewModel.AddActionToMenu(new PluginPart<IAdditionalAction>(host, action));
        }

        /// <summary>
        /// Удаляет действие из меню
        /// </summary>
        /// <param name="action"></param>
        public static void RemoveAction(IAdditionalAction action)
        {
            WindowManager.GetActiveWindow<MainWindow>()?.ViewModel.RemoveActionFromMenu(action.Guid);
        }

        /// <summary>
        /// Добавляет сервис перевода в список доступных сервисов
        /// </summary>
        /// <param name="translator">Сервис перевода</param>
        public static void AddTranslator(ITranslateService translator)
        {
            TranslateService.OnlineTranslators.Add(translator.Guid, new OneTranslationService(translator));
        }

        /// <summary>
        /// Удаляет сервис перевода из списка доступных сервисов
        /// </summary>
        /// <param name="translator">Сервис перевода</param>
        public static void RemoveTranslator(ITranslateService translator)
        {
            TranslateService.OnlineTranslators.Remove(translator.Guid);
        }

        /// <summary>
        /// Загружает плагины из папки (вызывается только 1 раз)
        /// </summary>
        public static void LoadPlugins()
        {
            if (_pluginsLoaded)
                return;

            if (Directory.Exists(GlobalVariables.PathToPlugins))
            {
                IEnumerable<string> plugins = Directory.EnumerateFiles(GlobalVariables.PathToPlugins, "*.dll", SearchOption.TopDirectoryOnly);

                plugins.ForEach(LoadPlugin);

                _pluginsLoaded = true;
            }

            LoadCurrentTranslationService();
        }

        /// <summary>
        /// Устанавливает текущий сервис перевода, основываясь на настройках
        /// </summary>
        private static void LoadCurrentTranslationService()
        {
            if (TranslateService.OnlineTranslators.TryGetValue(GlobalVariables.AppSettings.OnlineTranslator, out var found))
            {
                GlobalVariables.CurrentTranslationService.Value = found;
            }
            else
            {
                GlobalVariables.AppSettings.OnlineTranslator = TranslateService.OnlineTranslators.First().Key;
                GlobalVariables.CurrentTranslationService.Value = TranslateService.OnlineTranslators.First().Value;
            }
        }
    }
}
