namespace Puur
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class AggregateRepositoryExtensions
    {
        public static Task<TAggregate> GetById<TAggregate>(this AggregateRepository repository, string id, CancellationToken cancellationToken = default(CancellationToken))
            where TAggregate : IAggregateRoot, new() => repository.GetById<TAggregate>(id, int.MaxValue, cancellationToken);

        public static Task Save<TAggregate>(this AggregateRepository repository, TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
            where TAggregate : IAggregateRoot => repository.Save<TAggregate>(aggregate, null, cancellationToken);
    }
}