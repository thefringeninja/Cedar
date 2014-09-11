namespace Cedar.TypeResolution
{
    using System;
    using FluentAssertions;
    using Microsoft.Owin;
    using Xunit;

    public class TypeResolverTests
    {
        [Fact]
        public void Should_get_command_type()
        {
            var resolver = new DefaultRequestTypeResolver("cedar.tests", new[] {typeof (TypeResolverTests)});
            
            var context = new OwinContext();

            context.Request.ContentType = @"application/vnd.cedar.tests.commandtyperesolvertests+json";

            Type commandType = resolver.ResolveInputType(new CedarRequest(context));

            commandType.Should().NotBeNull();
        }
    }
}