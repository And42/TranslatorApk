using System;

namespace TranslatorApk.Logic.EventManagerLogic
{
    public class ManualEvent<EventParameter>
    {
        private event Action<EventParameter> manualEvent;

        public void Subscribe(Action<EventParameter> action)
        {
            manualEvent += action;
        }

        public void Unsubscribe(Action<EventParameter> action)
        {
            manualEvent -= action;
        }

        public void Publish(EventParameter parameter)
        {
            manualEvent?.Invoke(parameter);
        }
    }
}
