namespace Puur.EventStore
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using global::EventStore.ClientAPI.Exceptions;
    using Puur.EventSourcing;
    using Puur.Serialization;

    public class EventStoreAdapter : IEventStoreAdapter
    {
        private readonly GetEventStoreConnection _getEventStoreConnection;
        private readonly CreateEventData _createEventData;
        private readonly ResolveStreamEvent _resolveStreamEvent;

        public EventStoreAdapter(GetEventStoreConnection getEventStoreConnection, Serialize serialize, Deserialize deserialize)
        {
            _getEventStoreConnection = getEventStoreConnection;
            _createEventData         = (se, ct) => Defaults.CreateEventData(se, serialize, ct);
            _resolveStreamEvent      = (re, ct) => Defaults.ResolveStreamEvent(re, deserialize, ct);
        }

        public EventStoreAdapter(GetEventStoreConnection getEventStoreConnection, CreateEventData createEventData, ResolveStreamEvent resolveStreamEvent)
        {
            _getEventStoreConnection = getEventStoreConnection;
            _createEventData         = createEventData;
            _resolveStreamEvent      = resolveStreamEvent;
        }

        public async Task AppendStream(string streamName, long expectedVersion, IEnumerable<StreamEvent> streamEvents, CancellationToken cancellationToken = new CancellationToken())
        {
            var events = await Task
                .WhenAll(streamEvents.Select(streamEvent => _createEventData(streamEvent, cancellationToken)))
                .ConfigureAwait(false);

            using(var connection = _getEventStoreConnection())
            {
                try
                {
                    await connection
                        .AppendToStreamAsync(streamName, expectedVersion, events)
                        .ConfigureAwait(false);
                }
                catch(WrongExpectedVersionException ex)
                {
                    throw new WrongExpectedStreamVersionException(ex.Message, ex.InnerException);
                }
            }
        }

        public async Task<StreamEventsPage> ReadStream(
            string streamName, long fromVersion, long maxCount,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = new CancellationToken())
        {
            StreamEventsSlice slice;
            using(var connection = _getEventStoreConnection())
            {
                slice = await connection
                    .ReadStreamEventsForwardAsync(streamName, fromVersion, (int) maxCount, false)
                    .ConfigureAwait(false);
            }

            var events = await Task
                .WhenAll(slice.Events.Select(resolvedEvent => _resolveStreamEvent(resolvedEvent.Event, cancellationToken)))
                .ConfigureAwait(false);

            return new StreamEventsPage(
                streamName   : slice.Stream,
                fromVersion  : slice.FromEventNumber,
                nextVersion  : slice.NextEventNumber,
                lastVersion  : slice.LastEventNumber,
                events       : events,
                direction    : direction,
                isEndOfStream: slice.IsEndOfStream);
        }
    }
}