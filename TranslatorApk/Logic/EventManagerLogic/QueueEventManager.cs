using System;
using System.Collections.Generic;

namespace TranslatorApk.Logic.EventManagerLogic
{
    internal class QueueEventManager
    {
        internal enum ProcessingTypes
        {
            Collecting, Invoking
        }

        private readonly Queue<object> _eventsQueue = new Queue<object>();
        private readonly Dictionary<string, Action<object>> _handlersDictionary = new Dictionary<string, Action<object>>();

        public ProcessingTypes ProcessingType { get; private set; } = ProcessingTypes.Collecting;

        public void AddEvent<TEvent>(Action<TEvent> eventHandler)
        {
            if (eventHandler == null)
                throw new ArgumentNullException(nameof(eventHandler));

            _handlersDictionary.Add(typeof(TEvent).FullName ?? throw new InvalidOperationException(), param => eventHandler((TEvent)param));

            ManualEventManager.GetEvent<TEvent>().Subscribe(SubscribtionHandler);
        }

        public void RemoveEvent<TEvent>()
        {
            _handlersDictionary.Remove(typeof(TEvent).FullName ?? throw new InvalidOperationException());

            ManualEventManager.GetEvent<TEvent>().Unsubscribe(SubscribtionHandler);
        }

        public void SetProcessingType(ProcessingTypes processingType)
        {
            if (processingType == ProcessingTypes.Collecting)
            {
                ProcessingType = ProcessingTypes.Collecting;
            }
            else
            {
                for (int i = _eventsQueue.Count; i > 0; i--)
                    InvokeEvent(_eventsQueue.Dequeue());

                ProcessingType = ProcessingTypes.Invoking;
            }
        }

        private void SubscribtionHandler<TEvent>(TEvent param)
        {
            if (ProcessingType == ProcessingTypes.Collecting)
                _eventsQueue.Enqueue(param);
            else 
                InvokeEvent(param);
        }

        private void InvokeEvent(object eventObj)
        {
            if (_handlersDictionary.TryGetValue(eventObj.GetType().FullName ?? throw new InvalidOperationException(), out Action<object> handler))
                handler(eventObj);
        }
    }
}