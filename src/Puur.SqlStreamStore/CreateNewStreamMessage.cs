namespace Puur.SqlStreamStore
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::SqlStreamStore.Streams;
    using Puur.EventSourcing;

    public delegate Task<NewStreamMessage> CreateNewStreamMessage(StreamEvent streamEvent, CancellationToken cancellationToken);
}