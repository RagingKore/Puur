namespace Puur
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Puur.EventSourcing;
    using Puur.Messaging;

    public abstract class ApplicationService<TAggregate> : IMessageHandlerModule
        where TAggregate : class, IAggregateRoot, new()
    {
        private readonly Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>> _handlers 
            = new Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>>();

        private readonly AggregateRepository _repository;

        protected ApplicationService(AggregateRepository repository)
        {
            _repository = repository;
        }

        Dictionary<Type, Func<IMessage, IDictionary<string, string>, CancellationToken, Task>> IMessageHandlerModule.Handlers => _handlers;

        protected void When<T>(Func<T, IDictionary<string, string>, CancellationToken, Task> when) where T : IMessage 
            => _handlers.Add(typeof(T), (message, headers, ct) => when((T)message, headers, ct));

        protected void When<T>(
            Func<T, IDictionary<string, string>, string> getAggregateId, 
            Func<TAggregate, T, IDictionary<string, string>, CancellationToken, Task> act, 
            bool throwIfNotFound = true) where T : IMessage
        {
            _handlers.Add(typeof(T), async (message, headers, ct) =>
            {
                WrongExpectedStreamVersionException concurrencyException;
                do
                {
                    try
                    {
                        var aggregateId = getAggregateId((T)message, headers);

                        var aggregate = aggregateId == null
                            ? new TAggregate()
                            : await _repository
                                .GetById<TAggregate>(aggregateId, ct)
                                .ConfigureAwait(false);

                        if(throwIfNotFound && string.IsNullOrWhiteSpace(aggregate.Id))
                            throw new InvalidOperationException($"{typeof(TAggregate).Name} {aggregateId} not found!");

                        await act(aggregate,(T)message, headers, ct)
                            .ConfigureAwait(false);

                        await _repository
                            .Save(aggregate, headers, ct)
                            .ConfigureAwait(false);

                        concurrencyException = null;
                    }
                    catch(WrongExpectedStreamVersionException ex)
                    {
                        concurrencyException = ex;
                    }
                } while(concurrencyException != null);
            });
        }
    }

}