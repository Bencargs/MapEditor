﻿using System.Collections.Generic;
using System.Linq;

namespace MapEngine.Services.PathfindingService
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

        public List<T> ToList()
        {
            var results = new Stack<T>();
            var latest = this;
            while (latest != null)
            {
                results.Push(latest.Item);
                latest = latest.Previous;
            }
            return results.ToList();
        }
    }
}
