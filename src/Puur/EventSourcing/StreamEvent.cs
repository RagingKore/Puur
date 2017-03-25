namespace Puur.EventSourcing
{
    using System;
    using System.Collections.Generic;

    public class StreamEvent
    {
        public readonly Guid Id;
        public readonly object Event;
        public readonly IDictionary<string, string> Metadata;

        public StreamEvent(Guid id, object domainEvent, IDictionary<string, string> metadata)
        {
            Id       = id;
            Event    = domainEvent;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}