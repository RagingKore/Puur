namespace Puur.Projecting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Puur.Messaging;

    public interface IProjection
    {
        string Key { get; }
        bool CanHandle(IEvent e);
        Task Handle(IEvent e, IDictionary<string, string> headers, CancellationToken ct);
    }

    public abstract class Projection : IProjection
    {
        private readonly Dictionary<Type, Func<IEvent, IDictionary<string, string>, CancellationToken, Task>> _handlers
            = new Dictionary<Type, Func<IEvent, IDictionary<string, string>, CancellationToken, Task>>();

        protected void When<T>(Func<T, IDictionary<string, string>, CancellationToken, Task> when) where T : IEvent
            => _handlers.Add(typeof(T), (e, headers, ct) => when((T)e, headers, ct));

        string IProjection.Key => GetType().Name;

        bool IProjection.CanHandle(IEvent e)
            => _handlers.ContainsKey(e.GetType());

        Task IProjection.Handle(IEvent e, IDictionary<string, string> headers, CancellationToken ct)
            => _handlers[e.GetType()](e, headers,ct);
    }
}