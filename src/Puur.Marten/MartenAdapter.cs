namespace Puur.Marten
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten.Events;
    using global::Marten.Services;
    using Puur.EventSourcing;

    public class MartenAdapter : IEventStoreAdapter
    {
        private readonly GetDocumentSession _getDocumentSession;

        public MartenAdapter(GetDocumentSession getDocumentSession)
        {
            _getDocumentSession = getDocumentSession;
        }

        public Task AppendStream(string streamName, long expectedVersion, IEnumerable<StreamEvent> streamEvents, CancellationToken cancellationToken = new CancellationToken())
        {
            var events = streamEvents.Select(streamEvent => streamEvent.Event).ToArray();

            using(var session = _getDocumentSession())
            {
                try
                {
                    var expectedVersionAfterAppend = expectedVersion + events.Length;
                    session.Events.Append(GetStreamId(streamName), expectedVersionAfterAppend, events);
                }
                catch(EventStreamUnexpectedMaxEventIdException ex)
                {
                    throw new WrongExpectedStreamVersionException(ex.Message, ex.InnerException);
                }
            }

            return Task.CompletedTask;
        }

        public async Task<StreamEventsPage> ReadStream(
            string streamName, long fromVersion, long maxCount,
            StreamReadDirection direction = StreamReadDirection.Forward,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if(direction == StreamReadDirection.Backward)
                throw new NotSupportedException("Marten only supports forward reads.");

            if(fromVersion != 1)
                throw new NotSupportedException("Marten only supports reading streams forward from the beginning (version:1).");

            var streamId = GetStreamId(streamName);

            IReadOnlyList<IEvent> martenEvents;
            StreamState streamState;
            using(var session = _getDocumentSession())
            {
                var getEvents      = session.Events.FetchStreamAsync(streamId, (int) maxCount, null, cancellationToken);
                var getStreamState = session.Events.FetchStreamStateAsync(streamId, cancellationToken);

                await Task
                    .WhenAll(getEvents, getStreamState)
                    .ConfigureAwait(false);

                martenEvents = await getEvents;
                streamState  = await getStreamState;
            }

            var events = martenEvents.Select(martenEvent => new StreamEvent(martenEvent.Id, martenEvent.Data, new Dictionary<string, string>
            {
                //{ EventMetadataKeys.Id          , martenEvent.Id.ToString() },
                { EventMetadataKeys.Timestamp   , martenEvent.Timestamp.ToString(CultureInfo.InvariantCulture) },
                { EventMetadataKeys.Owner       , streamState.AggregateType.Name },
                //{ EventMetadataKeys.OwnerId     , martenEvent.. },
                { EventMetadataKeys.OwnerVersion, martenEvent.Version.ToString() },
                { EventMetadataKeys.ClrType     , martenEvent.Data.GetType().AssemblyQualifiedName },
            }));

            return new StreamEventsPage(
                streamName   : streamId.ToString(),
                fromVersion  : 1,
                nextVersion  : martenEvents.LastOrDefault()?.Version + 1 ?? 1,
                lastVersion  : streamState.Version,
                isEndOfStream: false,
                events       : events,
                direction    : StreamReadDirection.Forward);
        }

        private static Guid GetStreamId(string streamName) => !Guid.TryParse(streamName, out Guid streamId) 
            ? throw new NotSupportedException("Marten only supports Guids as stream names.") 
            : streamId;
    }
}