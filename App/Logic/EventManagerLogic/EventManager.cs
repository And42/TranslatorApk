using System.Collections.Generic;

namespace TranslatorApk.Logic.EventManagerLogic
{
    /// <summary>
    /// Обеспечивает взаимодействие с событиями приложения
    /// </summary>
    public static class ManualEventManager
    {
        private static readonly Dictionary<string, object> Events = new Dictionary<string, object>();

        /// <summary>
        /// Возвращает доступный для взаимодействия объект события
        /// </summary>
        /// <typeparam name="TEventType">Тип события</typeparam>
        public static PubSubEvent<TEventType> GetEvent<TEventType>()
        {
            string typeName = typeof(TEventType).FullName ?? string.Empty;

            if (Events.TryGetValue(typeName, out object @event))
                return (PubSubEvent<TEventType>) @event;

            var manualEvent = new PubSubEvent<TEventType>();

            Events.Add(typeName, manualEvent);

            return manualEvent;
        }
    }
}
