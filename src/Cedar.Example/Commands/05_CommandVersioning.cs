/*
 * Example mechanism resolving a command type from a command type name that has been 
 * parsed from the media type.
 * 
 * There are any number of ways this could be done including using relection,
 * conventions etc. In this example we will just create an explict map to lookup a
 * type from a key.
 */

// ReSharper disable once CheckNamespace
namespace Cedar.Example.Commands.CommandVersioning
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Commands;

    // 1. A number of versions of the same command.
    public class CreateTShirtV1
    {
        public string Name { get; set; }

        public string Size { get; set; }

    }

    public class CreateTShirtV2
    {
        public string Name { get; set; }

        public string[] Sizes { get; set; }
    }

    public class CreateTShirtV3
    {
        public string Name { get; set; }

        public string[] Sizes { get; set; }

        public string[] Colors { get; set; }
    }

    public class MyCommandModule : CommandHandlerModule
    {
        public MyCommandModule()
        {
            For<CreateTShirtV1>()
                .Handle((_, __) => { throw new NotSupportedException(); }); // 2. No longer support V1

            var createTShirtV3Handler = For<CreateTShirtV3>() // 3. Here we keep a reference to the V3 handler...
                .Handle((commandMessage, ct) => Task.FromResult(0));

            For<CreateTShirtV2>() 
                .Handle((commandMessage, ct) =>
                {
                    var command = new CreateTShirtV3
                    {
                        Name = commandMessage.Command.Name,
                        Sizes = commandMessage.Command.Sizes,
                        Colors = new []{ "Black" }
                    };
                    var upconvertedCommand = new CommandMessage<CreateTShirtV3>(
                        commandMessage.CommandId,
                        commandMessage.User,
                        command);
                    return createTShirtV3Handler(upconvertedCommand, ct); // 4. ... and invoke it here
                });
        }
    }
}