using System;
using System.Collections.Generic;
using System.Linq;

namespace MapEngine.Services.PathfindingService
{
    public class NodeQueue<TValue, TItem>
        where TValue : IComparable
    {
        private readonly Func<Node<TItem>, TValue> _valueSelector;
        private readonly SortedList<TValue, Node<TItem>> _items;

        public class KeyComparer : IComparer<TValue>
        {
            public int Compare(TValue x, TValue y)
            {
                var result = x.CompareTo(y);

                return result == 0 ? 1 : result;
            }
        }

        public NodeQueue(Func<Node<TItem>, TValue> valueSelector)
        {
            _valueSelector = valueSelector;
            var keyComparer = new KeyComparer();
            _items = new SortedList<TValue, Node<TItem>>(keyComparer);
        }

        public void Push(Node<TItem> item)
        {
            var value = _valueSelector(item);
            _items.Add(value, item);
        }

        public Node<TItem> Pop()
        {
            var min = _items.First().Value;
            _items.RemoveAt(0);

            return min;
        }

        public bool Any() => _items.Any();
    }
}
