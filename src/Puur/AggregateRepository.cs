namespace Puur
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Puur.EventSourcing;

    public class AggregateRepository
    {
        private readonly int _pageSizeReadBuffer;
        private readonly IEventStoreAdapter _eventStoreAdapter;
        private readonly GetStreamName _getStreamName;

        public AggregateRepository(IEventStoreAdapter eventStoreAdapter, GetStreamName getStreamName, int pageSizeReadBuffer = 200)
        {
            _eventStoreAdapter  = eventStoreAdapter;
            _getStreamName      = getStreamName;
            _pageSizeReadBuffer = pageSizeReadBuffer;
        }

        public async Task<TAggregate> GetById<TAggregate>(string id, long version, CancellationToken cancellationToken = default(CancellationToken))
            where TAggregate : IAggregateRoot, new()
        {
            if(string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));

            var streamName = _getStreamName(typeof(TAggregate), id);
            var pageStart  = 0L;
            var aggregate  = new TAggregate();

            StreamEventsPage currentPage;
            do
            {
                var eventCount = pageStart + _pageSizeReadBuffer <= version
                    ? _pageSizeReadBuffer
                    : version - pageStart + 1;

                currentPage = await _eventStoreAdapter
                    .ReadStream(streamName, pageStart, eventCount, StreamReadDirection.Forward, cancellationToken)
                    .ConfigureAwait(false);

                aggregate.LoadChanges(currentPage.Events.Select(streamEvent => streamEvent.Event));

                pageStart = currentPage.NextVersion;
            }
            while(version >= currentPage.NextVersion && !currentPage.IsEndOfStream);

            //Log.TraceFormat("Loaded {streamName} v{version}", streamName, aggregate.Version);

            return aggregate;
        }

        public async Task Save<TAggregate>(TAggregate aggregate, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default(CancellationToken))
            where TAggregate : IAggregateRoot
        {
            if(aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            if(!aggregate.GetChanges().Any())
                return;

            metadata = metadata ?? new Dictionary<string, string>();

            var streamEvents = aggregate
                .GetChanges()
                .Select((change, idx) => new StreamEvent(Guid.NewGuid(), change, metadata.Merge(new Dictionary<string, string>
                {
                    { EventMetadataKeys.Owner       , typeof(TAggregate).Name },
                    { EventMetadataKeys.OwnerId     , aggregate.Id },
                    { EventMetadataKeys.OwnerVersion, (aggregate.OriginalVersion+idx+1).ToString() },
                    { EventMetadataKeys.ClrType     , change.GetType().AssemblyQualifiedName },
                })));

            var streamName = _getStreamName(typeof(TAggregate), aggregate.Id);

            await _eventStoreAdapter
                .AppendStream(
                    streamName       : streamName,
                    expectedVersion  : aggregate.OriginalVersion-1,
                    streamEvents     : streamEvents,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            aggregate.AcceptChanges();

            //Log.TraceFormat("Saved {streamId} v{version}", streamName, aggregate.Version);
        }
    }
}