namespace Puur
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Puur.Messaging;

    public interface IAggregateState
    {
        string Id   { get; }
        int Version { get; }

        IEnumerable<object> Mutate(params object[] events);
    }

    public abstract class AggregateState : IAggregateState
    {
        private readonly Dictionary<Type, Action<IEvent>> _handlers = new Dictionary<Type, Action<IEvent>>();

        public virtual string Id   { get; protected set; }
        public virtual int Version { get; private set; }

        IEnumerable<object> IAggregateState.Mutate(params object[] events)
        {
            foreach(var e in events.Cast<IEvent>())
            {
                _handlers[e.GetType()](e);
                Version++;
            }
            return events;
        }

        protected void When<T>(Action<T> when) where T : IEvent => _handlers.Add(typeof(T), evt => when((T)evt));

        public override string ToString() => $"{Id} v{Version}";
    }
}