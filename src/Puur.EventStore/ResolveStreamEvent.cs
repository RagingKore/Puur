namespace Puur.EventStore
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Puur.EventSourcing;

    public delegate Task<StreamEvent> ResolveStreamEvent(RecordedEvent recordedEvent, CancellationToken cancellationToken);
}