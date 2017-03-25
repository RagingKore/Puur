namespace Puur.EventSourcing
{
    using System;

    public class WrongExpectedStreamVersionException : Exception
    {
        public WrongExpectedStreamVersionException(string message, Exception innerException = null) 
            : base(message, innerException) {}
    }
}