namespace Puur.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class MessageHandlerModule : IMessageHandlerModule
    {
        private readonly Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>> _handlers
            = new Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>>();

        Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>> IMessageHandlerModule.Handlers
            => _handlers;

        protected void When<T>(Func<T, IDictionary<string, string>, CancellationToken, Task> when) where T : IMessage
            => _handlers.Add(typeof(T), (message, headers, ct) => when((T)message, headers, ct));
    }
}