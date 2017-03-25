namespace Puur.EventSourcing
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventStoreAdapter
    {
        Task AppendStream(
            string streamName,
            long expectedVersion,
            IEnumerable<StreamEvent> streamEvents,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<StreamEventsPage> ReadStream(
            string streamName,
            long fromVersion,
            long maxCount,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}