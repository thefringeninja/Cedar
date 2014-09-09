namespace Cedar.Commands
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using Cedar.Commands.Fixtures;
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
                Func<Task> act = () => client.ExecuteCommand(new TestCommand(), Guid.NewGuid(), _fixture.CommandExecutionSettings);

                act.ShouldNotThrow();
            }
        }

        [Fact]
        public void When_execute_command_whose_handler_throws_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.ExecuteCommand(new TestCommandWhoseHandlerThrows(), Guid.NewGuid(), _fixture.CommandExecutionSettings);

                act.ShouldThrow<InvalidOperationException>();
            }
        }

        [Fact]
        public void When_command_endpoint_is_not_found_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var settings = new CommandExecutionSettings(
                    _fixture.CommandExecutionSettings.Vendor,
                    _fixture.CommandExecutionSettings.ModelToExceptionConverter,
                    "notfoundpath");

                Func<Task> act = () => client.ExecuteCommand(new TestCommand(), Guid.NewGuid(), settings);

                act.ShouldThrow<InvalidOperationException>();
            }
        }

        [Theory] // None of these are guid so the request should be passed through the middleware
        [InlineData("PUT", "D28B0541-8EBD-42FE-A42A")]
        [InlineData("PUT", "D28B0541-8EBD-42FE-A42A-71D503CF0646/")]
        [InlineData("GET", "D28B0541-8EBD-42FE-A42A-71D503CF0646")]
        [InlineData("GET", "D28B0541-8EBD-42FE-A42A-71D503CF0646/")]
        public async Task When_request_does_not_match_then_should_pass_through(string httpMethod, string commandId)
        {
            bool passedThrough = false;
            using (var client = _fixture.CreateHttpClient(env =>
            {
                passedThrough = true;
                return Task.FromResult(0);
            }))
            {
                var request = new HttpRequestMessage(
                    new HttpMethod(httpMethod),
                    _fixture.CommandExecutionSettings.Path + "/" + commandId);
                await client.SendAsync(request);
                passedThrough.Should().BeTrue();
            }
        }

        [Fact]
        public async Task When_request_is_not_json_then_should_get_Unsupported_Media_Type()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Put,
                    _fixture.CommandExecutionSettings.Path + "/" + Guid.NewGuid())
                {
                    Content = new StringContent("text")
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
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