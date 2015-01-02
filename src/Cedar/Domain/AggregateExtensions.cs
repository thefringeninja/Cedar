namespace Cedar.Domain
{
    using System;

    internal static class AggregateExtensions
    {
        public static void ThrowHandlerNotFound(this IAggregate aggregate, object eventMessage)
        {
            string exceptionMessage =
                "Aggregate of type '{0}' raised an event of type '{1}' but no handler could be found to handle the message."
                    .FormatWith(aggregate.GetType().Name, eventMessage.GetType().Name);

            throw new HandlerForDomainEventNotFoundException(exceptionMessage);
        }
    }
}