using System;

namespace TranslatorApk.Logic.EventManagerLogic
{
    public class PubSubEvent<T>
    {
        private event Action<T> ManualEvent;

        public void Subscribe(Action<T> handler)
        {
            ManualEvent += handler;
        }

        public void Unsubscribe(Action<T> handler)
        {
            ManualEvent -= handler;
        }

        public void Publish(T parameter)
        {
            ManualEvent?.Invoke(parameter);
        }
    }
}
