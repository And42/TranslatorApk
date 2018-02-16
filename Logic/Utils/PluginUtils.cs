using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Windows;
using TranslatorApkPluginLib;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Utils
{
    internal static class PluginUtils
    {
        private static bool _pluginsLoaded;

        /// <summary>
        /// Загружает плагин
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void LoadPlugin(string path)
        {
            AppDomain appDomain = 
                AppDomain.CreateDomain(Path.GetFileNameWithoutExtension(path) 
                ?? throw new NullReferenceException("Path name is null"));

            Type type = typeof(PluginHost);

            var loader = (PluginHost)
                appDomain.CreateInstanceAndUnwrap(
                    type.Assembly.FullName,
                    type.FullName ?? throw new NullReferenceException($"`{nameof(PluginHost)}.{nameof(type.FullName)}` is null")
                );

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

            AppDomain.Unload(found.Domain);

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
            if (TranslateService.OnlineTranslators.TryGetValue(SettingsIncapsuler.Instance.OnlineTranslator, out var found))
            {
                GlobalVariables.CurrentTranslationService = found;
            }
            else
            {
                SettingsIncapsuler.Instance.OnlineTranslator = TranslateService.OnlineTranslators.First().Key;
                GlobalVariables.CurrentTranslationService = TranslateService.OnlineTranslators.First().Value;
            }
        }
    }
}
