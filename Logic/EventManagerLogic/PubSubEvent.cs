using System;

namespace TranslatorApk.Logic.EventManagerLogic
{
    public class PubSubEvent<T>
    {
        private event Action<T> ManualEvent;

        public void Subscribe(Action<T> action)
        {
            ManualEvent += action;
        }

        public void Unsubscribe(Action<T> action)
        {
            ManualEvent -= action;
        }

        public void Publish(T parameter)
        {
            ManualEvent?.Invoke(parameter);
        }
    }
}
