namespace Puur.SqlStreamStore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using global::SqlStreamStore.Streams;
    using Puur.EventSourcing;
    using Puur.Serialization;

    internal static class Defaults
    {
        public static Task<NewStreamMessage> CreateNewStreamMessage(
            StreamEvent streamEvent,
            Serialize serialize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var newStreamMessage = new NewStreamMessage(
                messageId   : streamEvent.Id,
                type        : streamEvent.Event.GetType().Name,
                jsonData    : serialize(streamEvent.Event),
                jsonMetadata: serialize(streamEvent.Metadata));

            return Task.FromResult(newStreamMessage);
        }

        public static async Task<StreamEvent> ResolveStreamEvent(
            StreamMessage streamMessage,
            Deserialize deserialize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if(deserialize == null) throw new ArgumentNullException(nameof(deserialize));

            var metadata = (IDictionary<string, string>) deserialize(streamMessage.JsonMetadata, typeof(IDictionary<string, string>))
                           ?? new Dictionary<string, string>();

            metadata.Merge(new Dictionary<string, string>
            {
                //{ EventMetadataKeys.Id       , streamMessage.MessageId.ToString() },
                { EventMetadataKeys.Timestamp, streamMessage.CreatedUtc.ToString(CultureInfo.InvariantCulture) },
            });

            string messageTypeName;
            Type messageType;
            if(!metadata.TryGetValue(EventMetadataKeys.ClrType, out messageTypeName)
               || messageTypeName == null
               || (messageType = Type.GetType(messageTypeName, throwOnError: false)) == null)
            {
                return null;
            }

            var jsonData = await streamMessage.GetJsonData(cancellationToken);

            return new StreamEvent(
                streamMessage.MessageId,
                deserialize(jsonData, messageType),
                metadata);
        }
    }
}