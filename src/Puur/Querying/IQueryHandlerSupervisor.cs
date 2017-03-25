namespace Puur.Querying
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueryHandlerSupervisor
    {
        bool CanHandle(IQuery q);
        Task<TResult> Handle<TResult>(IQuery q, CancellationToken ct);
    }
}