namespace Cedar.Commands
{
    using System;
    using Cedar.Commands.ExceptionModels;
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
    }
}