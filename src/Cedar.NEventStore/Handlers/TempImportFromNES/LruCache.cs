namespace Cedar.NEventStore.Handlers.TempImportFromNES
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;

    internal class LruCache<TKey, TValue>
    {
        private readonly int _capacity;

        private readonly ConcurrentDictionary<TKey, Node> _nodes = new ConcurrentDictionary<TKey, Node>();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public LruCache(int capacity)
        {
            Debug.Assert(capacity > 10);
            _capacity = capacity;
        }

        public void Set(TKey key, TValue value)
        {
            var node = new Node
            {
                Value = value,
                Ticks = new Reference<long> { Value = _stopwatch.ElapsedTicks }
            };

            _nodes.AddOrUpdate(key, node, (_, __) => node);
            if (_nodes.Count > _capacity)
            {
                foreach (var source in _nodes.OrderBy(x => x.Value.Ticks).Take(_nodes.Count / 10))
                {
                    Node _;
                    _nodes.TryRemove(source.Key, out _);
                }
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            Node node;
            if (_nodes.TryGetValue(key, out node))
            {
                node.Ticks = new Reference<long> { Value = _stopwatch.ElapsedTicks };
                value = node.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        private class Node
        {
            public volatile Reference<long> Ticks;
            public TValue Value;
        }

        private class Reference<T> { public T Value; }
    }
}