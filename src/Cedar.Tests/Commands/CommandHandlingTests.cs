namespace Cedar.Commands
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;

    public class CommandHandlingTests : IUseFixture<CommandHandlingFixture>
    {
        private CommandHandlingFixture _fixture;

        [Fact]
        public void When_execute_valid_command_then_should_not_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.ExecuteCommand(new TestCommand(), Guid.NewGuid(), string.Empty);

                act.ShouldNotThrow();
            }
        }

        [Fact]
        public void When_execute_command_whose_handler_throws_standard_exception_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.ExecuteCommand(new TestCommandWhoseHandlerThrowsStandardException(), Guid.NewGuid(), string.Empty);

                act.ShouldThrow<HttpRequestException>();
            }
        }

        [Fact]
        public void When_execute_command_whose_handler_throws_http_problem_details_exception_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.ExecuteCommand(new TestCommandWhoseHandlerThrowProblemDetailsException(), Guid.NewGuid(), string.Empty);

                act.ShouldThrow<HttpProblemDetailsException>();
            }
        }

        [Fact]
        public void When_command_endpoint_is_not_found_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.ExecuteCommand(new TestCommand(), Guid.NewGuid(), "notfoundpath");

                act.ShouldThrow<HttpRequestException>();
            }
        }

        [Theory]
        [InlineData("text/html")]
        [InlineData("text/html+unsupported")]
        public async Task When_request_MediaType_does_not_have_a_valid_serialization_then_should_get_Unsupported_Media_Type(string mediaType)
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    Guid.NewGuid().ToString())
                {
                    Content = new StringContent("text")
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                var response = await client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
            }
        }

        public void SetFixture(CommandHandlingFixture data)
        {
            _fixture = data;
        }
    }
}