namespace Puur.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MessageHandlerSupervisor : IMessageHandlerSupervisor
    {
        private readonly Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>[]> _handlers;

        public MessageHandlerSupervisor(params IMessageHandlerModule[] modules)
        {
            _handlers = modules
                .SelectMany(m => m.Handlers)
                .GroupBy(h => h.Key)
                .ToDictionary(g => g.Key, g => g.Select(h => h.Value).ToArray());
        }

        public bool CanHandle(IMessage msg)
            => _handlers.ContainsKey(msg.GetType());

        public Task Handle(IMessage message, IDictionary<string, string> headers, CancellationToken ct)
            => _handlers[message.GetType()].Select(h => h(message, headers,ct)).WhenAll();
    }
}