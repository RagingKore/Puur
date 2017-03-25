namespace Puur.Querying
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class QueryService : IQueryService
    {
        private readonly IQueryHandlerSupervisor _supervisor;

        public QueryService(params IQueryHandlerModule[] modules)
        {
            _supervisor = new QueryHandlerSupervisor(modules);
        }

        public QueryService(IQueryHandlerSupervisor supervisor)
        {
            _supervisor = supervisor;
        }

        public Task<TResult> Run<TResult>(IQuery query, CancellationToken cancellationToken)
        {
            if(query == null)
                throw new ArgumentNullException(nameof(query));

            if(!_supervisor.CanHandle(query))
                throw new InvalidOperationException($"{query.GetType().Name} query has no registered handler.");

            return _supervisor.Handle<TResult>(query, cancellationToken);
        }
    }
}