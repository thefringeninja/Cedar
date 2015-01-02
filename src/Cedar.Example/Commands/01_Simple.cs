/*
 * Simplest example of wiring up a command, it's handler and the command
 * handling middleware
 */

// ReSharper disable once CheckNamespace
namespace Cedar.Example.Commands.Simple
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Commands;

    // 1. Simple command.
    public class MyCommand
    {}


    // 2. A service a command handler depends on.
    public interface IFoo
    {
        Task Bar();
    }

    // 3. Define your handlers.
    public class MyCommandModule : CommandHandlerModule
    {
        // Modules and handlers are singletons. Services that need to activated per request
        // should be injected as factory methods / funcs.
        public MyCommandModule(Func<IFoo> getFoo)
        {
            For<MyCommand>()
                .Handle(async (commandMessage, ct) =>
                {
                    var foo = getFoo();
                    await foo.Bar();
                });
        }
    }

    // 4. Wire it up
    public class Program
    {
        static void Main()
        {
            Func<IFoo> getFoo = () => new MyFoo();

            var resolver = new CommandHandlerResolver(new MyCommandModule(getFoo));
            var settings = new CommandHandlingSettings(resolver);

            var midFunc = CommandHandlingMiddleware.HandleCommands(settings);

            // 5. Add the midFunc to your owin pipeline
        }

        private class MyFoo : IFoo 
        {
            public Task Bar()
            {
                throw new NotImplementedException();
            }
        }
    }
}
