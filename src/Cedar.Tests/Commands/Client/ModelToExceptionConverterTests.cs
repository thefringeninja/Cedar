namespace Cedar.Commands.Client
{
    using System;
    using Cedar.Commands.ExceptionModels;
    using Cedar.ContentNegotiation.Client;
    using FluentAssertions;
    using Xunit;

    public class ModelToExceptionConverterTests
    {
        [Fact]
        public void Can_convert_ArgumentExceptionModel()
        {
            Exception exception = new ModelToExceptionConverter().Convert(new ArgumentExceptionModel());

            exception.Should().BeOfType<ArgumentException>();
        }

        [Fact]
        public void Can_convert_ArgumentNullExceptionModel()
        {
            Exception exception = new ModelToExceptionConverter().Convert(new ArgumentNullExceptionModel());

            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Can_convert_InvalidOperationException()
        {
            Exception exception = new ModelToExceptionConverter().Convert(new InvalidOperationExceptionModel());

            exception.Should().BeOfType<InvalidOperationException>();
        }

        [Fact]
        public void Can_convert_NotSupportedException()
        {
            Exception exception = new ModelToExceptionConverter().Convert(new NotSupportedExceptionModel());

            exception.Should().BeOfType<NotSupportedException>();
        }

        [Fact]
        public void Can_convert_DefaultException()
        {
            Exception exception = new ModelToExceptionConverter().Convert(new ExceptionModel { Message = "message"} );

            exception.Should().BeOfType<Exception>();
            exception.Message.Should().Be("message");
        }
    }
}