namespace Puur.Querying
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class QueryHandlerSupervisor : IQueryHandlerSupervisor
    {
        private readonly Dictionary<Type, Func<IQuery, CancellationToken, Task<object>>> _handlers;

        public QueryHandlerSupervisor(params IQueryHandlerModule[] modules)
        {
            _handlers = modules.SelectMany(m => m.Handlers).ToDictionary(h => h.Key, h => h.Value);
        }

        public bool CanHandle(IQuery q) => _handlers.ContainsKey(q.GetType());

        public async Task<TResult> Handle<TResult>(IQuery q, CancellationToken ct)
            => (TResult)await _handlers[q.GetType()](q, ct).ConfigureAwait(false);
    }
}