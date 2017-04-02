using System.Collections.Generic;

namespace TranslatorApk.Logic.EventManagerLogic
{
    public static class ManualEventManager
    {
        private static readonly Dictionary<string, object> events = new Dictionary<string, object>();

        public static ManualEvent<EventType> GetEvent<EventType>()
        {
            string typeName = typeof(EventType).FullName;

            if (events.TryGetValue(typeName, out object evnt))
            {
                return (ManualEvent<EventType>) evnt;
            }

            var manualEvent = new ManualEvent<EventType>();

            events.Add(typeName, manualEvent);

            return manualEvent;
        }
    }
}
