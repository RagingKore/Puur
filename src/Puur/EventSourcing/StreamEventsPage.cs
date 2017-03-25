namespace Puur.EventSourcing
{
    using System.Collections.Generic;
    using System.Linq;

    public class StreamEventsPage
    {
        public readonly string StreamName;
        public readonly long FromVersion;
        public readonly long NextVersion;
        public readonly long LastVersion;
        public readonly bool IsEndOfStream;
        public readonly IReadOnlyCollection<StreamEvent> Events;
        public readonly StreamReadDirection Direction;

        public StreamEventsPage(
            string streamName,
            long fromVersion,
            long nextVersion,
            long lastVersion,
            bool isEndOfStream,
            IEnumerable<StreamEvent> events,
            StreamReadDirection direction)
        {
            IsEndOfStream = isEndOfStream;
            StreamName    = streamName;
            FromVersion   = fromVersion;
            NextVersion   = nextVersion;
            LastVersion   = lastVersion;
            Events        = events.ToArray();
            Direction     = direction;
        }
    }
}