namespace Puur.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageHandlerModule
    {
        Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>> Handlers { get; }
    }
}