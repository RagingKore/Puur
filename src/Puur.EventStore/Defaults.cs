namespace Puur.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using Puur.EventSourcing;
    using Puur.Serialization;

    internal static class Defaults
    {
        public static Task<EventData> CreateEventData(
            StreamEvent streamEvent,
            Serialize serialize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventData = new EventData(
                eventId : streamEvent.Id,
                type    : streamEvent.Event.GetType().Name,
                isJson  : true,
                data    : Encoding.UTF8.GetBytes(serialize(streamEvent.Event)),
                metadata: Encoding.UTF8.GetBytes(serialize(streamEvent.Metadata)));

            return Task.FromResult(eventData);
        }

        public static Task<StreamEvent> ResolveStreamEvent(
            RecordedEvent recordedEvent,
            Deserialize deserialize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if(deserialize == null) throw new ArgumentNullException(nameof(deserialize));

            var metadata = (IDictionary<string, string>) deserialize(Encoding.UTF8.GetString(recordedEvent.Metadata), typeof(IDictionary<string, string>))
                           ?? new Dictionary<string, string>();

            metadata.Merge(new Dictionary<string, string>
            {
                //{ EventMetadataKeys.Id       , recordedEvent.EventId.ToString() },
                { EventMetadataKeys.Timestamp, recordedEvent.Created.ToString(CultureInfo.InvariantCulture) },
            });

            string messageTypeName;
            Type messageType;
            if(!metadata.TryGetValue(EventMetadataKeys.ClrType, out messageTypeName)
               || messageTypeName == null
               || (messageType = Type.GetType(messageTypeName, throwOnError: false)) == null)
            {
                return null;
            }

            var streamEvent = new StreamEvent(
                id         : recordedEvent.EventId,
                domainEvent: deserialize(Encoding.UTF8.GetString(recordedEvent.Data), messageType),
                metadata   : metadata);

            return Task.FromResult(streamEvent);
        }
    }
}