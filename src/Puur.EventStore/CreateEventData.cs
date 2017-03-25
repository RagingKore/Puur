namespace Puur.EventStore
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Puur.EventSourcing;

    public delegate Task<EventData> CreateEventData(StreamEvent streamEvent, CancellationToken cancellationToken);
}