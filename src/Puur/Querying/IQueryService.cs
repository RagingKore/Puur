namespace Puur.Querying
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueryService
    {
        Task<TResult> Run<TResult>(IQuery query, CancellationToken cancellationToken = default(CancellationToken));
    }
}