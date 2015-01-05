/*
 * Piping is a way to compose a handler piple line, allowing a handler be composed of 
 * smaller and potentially reusable operations.
 */

// ReSharper disable once CheckNamespace
namespace Cedar.Example.Commands.Piping
{
    using System.Threading.Tasks;
    using Cedar.Commands;

    public class Command
    {}

    public class CommandModule : CommandHandlerModule
    {
        public CommandModule()
        {
            // 1. Here we use a pipe to perform a command validation operation
            For<Command>()
                .Pipe(next => (commandMessage, ct) =>
                {
                    Validate(commandMessage.Command);
                    return next(commandMessage, ct);
                })
                .Handle((commandMessage, ct) => Task.FromResult(0));
        }

        private static void Validate<TCommand>(TCommand command)
        {}
    }
}
