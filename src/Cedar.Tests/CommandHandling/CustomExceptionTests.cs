namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.Client;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.Exceptions;
    using FluentAssertions;
    using Xunit;

    public class CustomExceptionTests
    {
        [Fact]
        public void When_execute_valid_command_then_should_not_throw()
        {
            using (var host = new CedarHost(new TestBootstrapper()))
            {
                using (CedarClient client = host.CreateClient(new CustomModelToExceptionConverter()))
                {
                    Func<Task> act = () => client.ExecuteCommand("cedar", new TestCommand(), Guid.NewGuid());

                    act.ShouldThrow<CustomException>()
                        .Where(ex => ex.Message == "custom");
                }
            }
        }

        public class TestBootstrapper : CedarBootstrapper
        {
            public override string VendorName
            {
                get { return "cedar"; }
            }

            public override IEnumerable<Type> CommandHandlerTypes
            {
                get { return new[] {typeof (TestCommandHandler)}; }
            }

            public override IExceptionToModelConverter ExceptionToModelConverter
            {
                get { return new CustomExceptionToModelConverter(); }
            }
        }

        public class TestCommand
        {}

        public class TestCommandHandler : ICommandHandler<TestCommand>
        {
            public Task Handle(ICommandContext context, TestCommand command)
            {
                throw new CustomException("custom");
            }
        }

        public class CustomException : Exception
        {
            public CustomException(string message)
                : base(message)
            {}
        }

        public class CustomExceptionModel : ExceptionModel
        {}

        public class CustomExceptionToModelConverter : ExceptionToModelConverter
        {
            public override ExceptionModel Convert(Exception exception)
            {
                ExceptionModel model = null;
                TypeSwitch
                    .On(exception)
                    .Case<CustomException>(ex => model = new CustomExceptionModel())
                    .Default(() => model = base.Convert(exception));
                model.Message = exception.Message;
                model.StackTrace = exception.StackTrace;
                return model;
            }
        }

        public class CustomModelToExceptionConverter : ModelToExceptionConverter
        {
            public override Exception Convert(ExceptionModel model)
            {
                Exception exception = null;
                TypeSwitch.On(model)
                    .Case<CustomExceptionModel>(m => exception = new CustomException(m.Message))
                    .Default(m => exception = base.Convert(m));

                return exception;
            }
        }
    }
}