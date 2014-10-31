namespace Cedar.Commands
{
    using System;
    using System.Net;
    using Cedar.ExceptionModels;
    using Cedar.ExceptionModels.Client;
    using FluentAssertions;
    using Xunit;

    public class ExceptionToModelConverterTests
    {
        [Fact]
        public void When_converting_ArgumentNullException_then_should_have_paramater_name()
        {
            ExceptionModel exceptionModel = new ExceptionToModelConverter()
                .Convert(new ArgumentNullException("param"));

            exceptionModel.Should().BeOfType<ArgumentExceptionModel>();
            ((ArgumentExceptionModel)exceptionModel).ParamName.Should().Be("param");
        }

        [Fact]
        public void When_converting_NotSupportedException_then_should_get_NotSupportedExceptionModel()
        {
            ExceptionModel exceptionModel = new ExceptionToModelConverter()
                .Convert(new NotSupportedException("message"));

            exceptionModel.Should().BeOfType<NotSupportedExceptionModel>();
            exceptionModel.Message.Should().Be("message");
        }

        [Fact]
        public void When_converting_default_exception_then_should_get_ExceptionModel()
        {
            ExceptionModel exceptionModel = new ExceptionToModelConverter()
                .Convert(new Exception("message"));

            exceptionModel.Should().BeOfType<ExceptionModel>();
            exceptionModel.Message.Should().Be("message");
        }

        [Fact(Skip = "Need to talk about this.")]
        public void When_converting_HttpStatusException_then_should_get_ExceptionModel_of_InnerException()
        {
            ExceptionModel exceptionModel = new ExceptionToModelConverter()
                .Convert(new HttpStatusException("message", HttpStatusCode.NotAcceptable, new InvalidOperationException("a different message")));

            exceptionModel.Should().BeOfType<InvalidOperationExceptionModel>();
            exceptionModel.Message.Should().Be("a different message");
        }
    }
}