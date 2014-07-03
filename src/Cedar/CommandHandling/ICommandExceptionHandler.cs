namespace Cedar.CommandHandling
{
    using System;
    using Nancy.Responses.Negotiation;

    public interface ICommandExceptionHandler
    {
        bool Handles(Exception ex);

        Negotiator Handle(Exception ex, Negotiator negotiator);
    }
}