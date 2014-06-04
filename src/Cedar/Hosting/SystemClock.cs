namespace Cedar.Hosting
{
    using System;

    public class SystemClock : ISystemClock
    {
        private int _lastTicks = -1;
        private DateTimeOffset _lastUtcDateTime = DateTimeOffset.MinValue;
        private readonly DateTimeOffset _start = DateTimeOffset.UtcNow;
        private readonly DateTimeOffset _systemUtcTime;

        public SystemClock(DateTimeOffset systemUtcTime)
        {
            _systemUtcTime = systemUtcTime;
        }

        public DateTimeOffset UtcNow
        {
            get
            {
                // Accessing DateTimeOffset.UtcNow is suprisingly costly. This
                // is a an optimization.
                int tickCount = Environment.TickCount;
                if (tickCount == _lastTicks)
                {
                    return _lastUtcDateTime;
                }
                TimeSpan progressed = (DateTimeOffset.UtcNow - _start);
                _lastUtcDateTime = _systemUtcTime + progressed;
                _lastTicks = tickCount;
                return _lastUtcDateTime;
            }
        }
    }
}