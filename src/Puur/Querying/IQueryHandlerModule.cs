namespace Puur.Querying
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueryHandlerModule
    {
        Dictionary<Type, Func<IQuery, CancellationToken, Task<object>>> Handlers { get; }
    }
}