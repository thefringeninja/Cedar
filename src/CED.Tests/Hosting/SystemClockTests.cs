namespace CED.Hosting
{
    using System;
    using CED.Annotations;
    using FluentAssertions;
    using Xunit;

    [UsedImplicitly]
    public class SystemClockTests
    {
        private readonly SystemClock _systemClock;
        private readonly DateTimeOffset _initialDateTimeOffset;
        private DateTimeOffset _dateTimeOffset;

        public SystemClockTests()
        {
            _initialDateTimeOffset = new DateTimeOffset(2200, 1, 1, 0, 0, 0, TimeSpan.Zero);
            _systemClock = new SystemClock(_initialDateTimeOffset);
        }

        [Fact]
        public void When_getting_the_datetime_it_should_be_on_or_after_inital_datetime()
        {
            _dateTimeOffset = _systemClock.UtcNow;

            _dateTimeOffset.CompareTo(_initialDateTimeOffset).Should().BeGreaterOrEqualTo(0);
        }
    }
}
