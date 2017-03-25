namespace Puur.EventSourcing
{
    using System;

    public delegate string GetStreamName(Type aggregateType, string aggregateId);
}