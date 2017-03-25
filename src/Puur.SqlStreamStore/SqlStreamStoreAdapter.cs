
namespace Puur.SqlStreamStore
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::SqlStreamStore;
    using global::SqlStreamStore.Streams;
    using Puur.EventSourcing;
    using Puur.Serialization;

    public class SqlStreamStoreAdapter : IEventStoreAdapter
    {
        private readonly IStreamStore _streamStore;
        private readonly CreateNewStreamMessage _createNewStreamMessage;
        private readonly ResolveStreamEvent _resolveStreamEvent;

        public SqlStreamStoreAdapter(IStreamStore streamStore, Serialize serialize, Deserialize deserialize)
        {
            _streamStore            = streamStore;
            _createNewStreamMessage = (se, ct) => Defaults.CreateNewStreamMessage(se, serialize, ct);
            _resolveStreamEvent     = (sm, ct) => Defaults.ResolveStreamEvent(sm, deserialize, ct);
        }

        public SqlStreamStoreAdapter(IStreamStore streamStore, CreateNewStreamMessage createNewStreamMessage, ResolveStreamEvent resolveStreamEvent)
        {
            _streamStore            = streamStore;
            _createNewStreamMessage = createNewStreamMessage;
            _resolveStreamEvent     = resolveStreamEvent;
        }

        public async Task AppendStream(string streamName, long expectedVersion, IEnumerable<StreamEvent> streamEvents, CancellationToken cancellationToken = default(CancellationToken))
        {
            var events = await Task
                .WhenAll(streamEvents.Select(streamEvent => _createNewStreamMessage(streamEvent, cancellationToken)))
                .ConfigureAwait(false);

            try
            {
                await _streamStore
                    .AppendToStream(streamName, (int)expectedVersion, events, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch(WrongExpectedVersionException ex)
            {
                throw new WrongExpectedStreamVersionException(ex.Message, ex.InnerException);
            }
        }

        public async Task<StreamEventsPage> ReadStream(
            string streamName, long fromVersion, long maxCount,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var page   = await _streamStore
                .ReadStreamForwards(streamName, (int)fromVersion, (int)maxCount, cancellationToken)
                .ConfigureAwait(false);

            var events = await Task
                .WhenAll(page.Messages.Select(streamMessage => _resolveStreamEvent(streamMessage, cancellationToken)))
                .ConfigureAwait(false);

            return new StreamEventsPage(
                streamName   : page.StreamId,
                fromVersion  : page.FromStreamVersion,
                nextVersion  : page.NextStreamVersion,
                lastVersion  : page.LastStreamVersion,
                isEndOfStream: page.IsEnd,
                events       : events,
                direction    : direction);
        }
    }

}