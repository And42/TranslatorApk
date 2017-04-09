using System;

namespace TranslatorApk.Logic.EventManagerLogic
{
    public class PubSubEvent<T>
    {
        private event Action<T> manualEvent;

        public void Subscribe(Action<T> action)
        {
            manualEvent += action;
        }

        public void Unsubscribe(Action<T> action)
        {
            manualEvent -= action;
        }

        public void Publish(T parameter)
        {
            manualEvent?.Invoke(parameter);
        }
    }
}
