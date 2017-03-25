namespace Puur.Messaging {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageHandlerSupervisor
    {
        bool CanHandle(IMessage message);
        Task Handle(IMessage message, IDictionary<string, string> headers, CancellationToken ct);
    }
}