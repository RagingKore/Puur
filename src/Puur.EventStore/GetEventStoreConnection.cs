namespace Puur.EventStore
{
    using global::EventStore.ClientAPI;

    public delegate IEventStoreConnection GetEventStoreConnection();
}