namespace Puur.Messaging
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMediator : ICommandDispatcher, IEventDispatcher {}

    public interface ICommandDispatcher
    {
        Task Send(ICommand cmd, IDictionary<string, string> headers = null, CancellationToken cancellationToken = new CancellationToken());
    }

    public interface IEventDispatcher
    {
        Task Publish(IEvent evt, IDictionary<string, string> headers = null, CancellationToken cancellationToken = new CancellationToken());
    }

    public interface IMessageDispatcher
    {
        Task Dispatch(IMessage msg, IDictionary<string, string> headers = null, CancellationToken cancellationToken = new CancellationToken());
    }
}