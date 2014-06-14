namespace Cedar.Example.Tests.CommandVersioning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Client;
    using Example.CommandVersioning;
    using FluentAssertions;
    using Xunit;

    public class CommandVersioningTests : IDisposable
    {
        private readonly CedarClient _client;
        private readonly CedarHost _host;
        private readonly IList<object> _publishedEvents = new List<object>();
        private ReplaySubject<object> _events;

        public CommandVersioningTests()
        {
            _events = new ReplaySubject<object>();
            _events.Subscribe(_publishedEvents.Add);
            _host = new CedarHost(new Bootstrapper(new ObservableMessagePublisher(_events)));
            _client = _host.CreateClient();
        }

        #region IDisposable Members

        public void Dispose()
        {
            _host.Dispose();
            _client.Dispose();
        }

        #endregion

        [Fact]
        public void version_1_is_not_supported()
        {
            Func<Task> act = () =>
                _client.ExecuteCommand(
                    new CreateTShirt
                    {
                        Name = "I Love Bacon",
                        Size = "XL"
                    },
                    Guid.NewGuid());

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void version_2_is_not_backwards_compatible()
        {
            Func<Task> act = () =>
                _client.ExecuteCommand(
                    new CreateTShirtV2
                    {
                        Name = "I Love Bacon",
                        Sizes = new[] {"XL"}
                    },
                    Guid.NewGuid());

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void version_3_upgrades_to_version_4()
        {
            Func<Task> act = () =>
                _client.ExecuteCommand(
                    new CreateTShirtV3
                    {
                        Name = "I Love Bacon",
                        Sizes = new[] {"XL"},
                        Colors = new[] {"Red"}
                    },
                    Guid.NewGuid());

            act.ShouldNotThrow();
            var @event = _publishedEvents.Single() as TShirtCreatedV4;
            @event.Name.Should().Be("I Love Bacon");
            @event.Sizes.SequenceEqual(new[] {"XL"}).Should().BeTrue();
            @event.Colors.SequenceEqual(new[] {"Red"}).Should().BeTrue();
            @event.BlankType.Should().Be("Round");
        }

        [Fact]
        public void version_4_just_works()
        {
            Func<Task> act = () =>
                _client.ExecuteCommand(
                    new CreateTShirtV4
                    {
                        Name = "I Love Bacon",
                        Sizes = new[] {"XL"},
                        Colors = new[] {"Red"},
                        BlankType = "Athletic"
                    },
                    Guid.NewGuid());

            act.ShouldNotThrow();
            var @event = _publishedEvents.Single() as TShirtCreatedV4;
            @event.Name.Should().Be("I Love Bacon");
            @event.Sizes.SequenceEqual(new[] {"XL"}).Should().BeTrue();
            @event.Colors.SequenceEqual(new[] {"Red"}).Should().BeTrue();
            @event.BlankType.Should().Be("Athletic");
        }
    }
}
