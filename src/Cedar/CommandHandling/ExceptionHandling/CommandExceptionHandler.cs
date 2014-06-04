namespace Cedar.CommandHandling.ExceptionHandling
{
    using System;
    using Nancy.Responses.Negotiation;

    public abstract class CommandExceptionHandler<TException> : ICommandExceptionHandler
        where TException : Exception
    {
        public bool Handles(Exception ex)
        {
            return ex.GetType() == typeof (TException);
        }

        public Negotiator Handle(Exception ex, Negotiator negotiator)
        {
            return Handle((TException)ex, negotiator);
        }

        protected abstract Negotiator Handle(TException exception, Negotiator negotiator);
    }
}