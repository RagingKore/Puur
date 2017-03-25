namespace Puur.Querying
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class QueryHandlerModule : IQueryHandlerModule
    {
        private readonly Dictionary<Type, Func<IQuery, CancellationToken, Task<object>>> _handlers
            = new Dictionary<Type, Func<IQuery, CancellationToken, Task<object>>>();

        Dictionary<Type, Func<IQuery, CancellationToken, Task<object>>> IQueryHandlerModule.Handlers
            => _handlers;

        protected void When<T, TResult>(Func<T, CancellationToken, Task<TResult>> when) where T : IQuery
            => _handlers.Add(typeof(T), async (q, ct) => await when((T)q, ct).ConfigureAwait(false));
    }
}