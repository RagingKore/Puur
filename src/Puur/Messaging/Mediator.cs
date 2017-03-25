namespace Puur.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Mediator : IMediator
    {
        //private static readonly ILog Log = LogProvider.For<Mediator>();

        private readonly IMessageHandlerSupervisor _supervisor;

        public Mediator(params IMessageHandlerModule[] modules)
        {
            _supervisor = new MessageHandlerSupervisor(modules);
        }

        public Mediator(IMessageHandlerSupervisor supervisor)
        {
            _supervisor = supervisor;
        }

        public Task Send(ICommand message, IDictionary<string, string> headers = null, CancellationToken cancellationToken = new CancellationToken())
        {
            if(message == null)
                throw new ArgumentNullException(nameof(message));

            if(!_supervisor.CanHandle(message))
                throw new InvalidOperationException($"{message.GetType().Name} command has no registered handler.");

            return _supervisor.Handle(message, headers, cancellationToken);
        }

        public Task Publish(IEvent message, IDictionary<string, string> headers = null, CancellationToken cancellationToken = new CancellationToken())
        {
            if(message == null)
                throw new ArgumentNullException(nameof(message));

//            if(!_supervisor.CanHandle(message))
//                Log.WarnFormat("{eventTypeName} event has no registered handler.", message.GetType().Name);

            return _supervisor.Handle(message, headers, cancellationToken);
        }
    }

}