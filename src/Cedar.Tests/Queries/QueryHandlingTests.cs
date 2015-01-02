namespace Cedar.Queries
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.Queries.Client;
    using Cedar.Queries.Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;

    public class QueryHandlingTests : IUseFixture<QueryHandlingFixture>
    {
        private QueryHandlingFixture _fixture;

        [Fact]
        public void When_execute_valid_query_then_should_not_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task<TestQueryResponse>> act = () => client.ExecuteQuery<TestQuery, TestQueryResponse>(new TestQuery(), Guid.NewGuid(), _fixture.MessageExecutionSettings);

                act.ShouldNotThrow();
            }
        }

        [Fact]
        public void When_execute_query_whose_handler_throws_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task<TestQueryResponse>> act = () => client
                    .ExecuteQuery<TestQueryWhoseHandlerThrows, TestQueryResponse>(new TestQueryWhoseHandlerThrows(), Guid.NewGuid(), _fixture.MessageExecutionSettings);

                act.ShouldThrow<InvalidOperationException>();
            }
        }

        [Fact]
        public void When_query_endpoint_is_not_found_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var settings = new QueryExecutionSettings(
                    _fixture.MessageExecutionSettings.ModelToExceptionConverter,
                    path: "notfoundpath");

                Func<Task<TestQueryResponse>> act = () => client
                    .ExecuteQuery<TestQuery, TestQueryResponse>(new TestQuery(), Guid.NewGuid(), settings);

                act.ShouldThrow<InvalidOperationException>();
            }
        }

        [Theory]
        [InlineData("PUT", "TestQuery")]
        [InlineData("PUT", "not-important")]
        [InlineData("DELETE", "TestQuery")]
        [InlineData("DELETE", "not-important")]
        public async Task When_request_method_does_not_match_then_should_pass_through(string httpMethod, string query)
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
                    _fixture.MessageExecutionSettings.Path + "/" + query)
                {
                    Content = new StringContent("{}")
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/vnd.vendor.testquery+json")
                        }
                    }
                };

                request.Headers.Accept.ParseAdd("application/vnd.vendor.testqueryresponse+json");
                await client.SendAsync(request);
                passedThrough.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("not-important")]
        public async Task When_request_path_does_not_match_then_should_return_not_found(string query)
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _fixture.MessageExecutionSettings.Path + "/" + query)
                {
                    Content = new StringContent("{}")
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    }
                };

                request.Headers.Accept.ParseAdd("application/vnd.vendor.testqueryresponse+json");
                var response = await client.SendAsync(request);
                response.StatusCode.Should().Match(code => (int)code >= 400 && (int)code < 500);
            }
        }

        [Theory]
        [InlineData("testquery")]
        public async Task When_request_if_non_match_matches_then_should_return_not_modified(string query)
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _fixture.MessageExecutionSettings.Path + "/" + query);

                request.Headers.Accept.ParseAdd("application/vnd.vendor.testqueryresponse+json");
                request.Headers.IfNoneMatch.ParseAdd(string.Format("\"{0}\"", "{}".GetHashCode()));
                var response = await client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.NotModified);
            }
        }

        [Theory]
        [InlineData("testquery")]
        public async Task When_request_if_non_match_not_matching_then_should_return_ok(string query)
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _fixture.MessageExecutionSettings.Path + "/" + query);

                request.Headers.Accept.ParseAdd("application/vnd.vendor.testqueryresponse+json");
                request.Headers.IfNoneMatch.ParseAdd("\"old-etag\"");
                var response = await client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task When_corresponding_response_type_is_not_found_should_return_not_acceptable()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _fixture.MessageExecutionSettings.Path + "/testquerywithnoreturntype");

                request.Headers.Accept.ParseAdd("application/vnd.vendor.testquerywithnoreturntyperesponse+json");
                
                var response = await client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);
            }
        }

        [Theory]
        [InlineData("testquerywhichreturnsnull")]
        public async Task When_request_can_not_be_found_it_should_return_not_found(string query)
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _fixture.MessageExecutionSettings.Path + "/" + query);

                request.Headers.Accept.ParseAdd("application/vnd.vendor.testquerywhichreturnsnullresponse+json");

                var response = await client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        public void SetFixture(QueryHandlingFixture data)
        {
            _fixture = data;
        }
    }
}