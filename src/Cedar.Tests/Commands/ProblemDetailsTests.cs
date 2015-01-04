namespace Cedar.Commands
{
    using System;
    using System.Net;
    using FluentAssertions;
    using Xunit;

    public class ProblemDetailsTests
    {
        [Fact]
        public void Can_create_exception_with_status_code()
        {
            var sut = new HttpProblemDetails(HttpStatusCode.BadRequest);

            sut.Status.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void When_setting_type_with_a_relative_uri_should_throw()
        {
            var sut = new HttpProblemDetails(HttpStatusCode.BadRequest);

            Action act = () => sut.Type = new Uri("/relateive", UriKind.Relative);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void When_setting_instance_with_a_relative_uri_should_throw()
        {
            var sut = new HttpProblemDetails(HttpStatusCode.BadRequest);

            Action act = () => sut.Instance = new Uri("/relateive", UriKind.Relative);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Can_json_serialize_and_deserialize()
        {
            
        }
    }
}