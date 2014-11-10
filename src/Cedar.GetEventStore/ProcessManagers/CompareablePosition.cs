namespace Cedar.GetEventStore.ProcessManagers
{
    using System;
    using Cedar.GetEventStore.Handlers;
    using EventStore.ClientAPI;

    public class CompareablePosition : IComparable<string>
    {
        private readonly Position? _position;

        public CompareablePosition(Position? position = default(Position?))
        {
            _position = position;
        }

        public int CompareTo(string other)
        {
            var otherPosition = other.ParsePosition();
            if (false ==_position.HasValue && false == otherPosition.HasValue)
            {
                return 0;
            }

            if(false == _position.HasValue)
            {
                return -1;
            }

            if(false == otherPosition.HasValue)
            {
                return 1;
            }

            if(_position.Value == otherPosition.Value)
            {
                return 0;
            }

            return _position.Value < otherPosition.Value ? -1 : 1;
        }
    }
}