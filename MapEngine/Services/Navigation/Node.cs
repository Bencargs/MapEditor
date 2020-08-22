using System.Collections.Generic;

namespace MapEngine.Services.Navigation
{
    public class Node<T>
    {
        public Node<T> Previous { get; set; }
        public T Item { get; set; }
        public float Value { get; set; }

        public Node(T source)
        {
            Item = source;
        }

        public T[] ToArray()
        {
            var results = new Stack<T>();
            var latest = this;
            while (latest != null)
            {
                results.Push(latest.Item);
                latest = latest.Previous;
            }
            return results.ToArray();
        }
    }
}
