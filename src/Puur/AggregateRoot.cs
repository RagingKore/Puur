namespace Puur
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IAggregateRoot
    {
        string Id { get; }
        int Version { get; }
        int OriginalVersion { get; }

        IEnumerable<object> GetChanges();
        IAggregateState GetState();
        void AcceptChanges();

        void LoadState(object state);
        void LoadChanges(IEnumerable<object> events);
    }

    public abstract class AggregateRoot<TState> : IAggregateRoot
        where TState : IAggregateState, new()
    {
        protected TState State = new TState();
        private readonly List<object> _changes = new List<object>();

        public string Id                                            => State.Id;
        public int Version                                          => State.Version;
        public int OriginalVersion                                  => State.Version - _changes.Count;

        IEnumerable<object> IAggregateRoot.GetChanges()             => _changes.ToArray();
        IAggregateState IAggregateRoot.GetState()                   => State;
        void IAggregateRoot.AcceptChanges()                         => _changes.Clear();

        void IAggregateRoot.LoadState(object state)                 => State = (TState)state;
        void IAggregateRoot.LoadChanges(IEnumerable<object> events) => State.Mutate(events.ToArray());

        protected void Apply(object e)                              => _changes.AddRange(State.Mutate(e));
        public override string ToString()                           => State.ToString();
    }
}