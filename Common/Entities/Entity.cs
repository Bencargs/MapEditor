using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    public class Entity
    {
        public int Id { get; set; }
        public List<IComponent> Components { get; set; } = new List<IComponent>();

        public T GetComponent<T>()
            where T : IComponent
        {
            var component = GetComponents<T>().LastOrDefault();
            return component;
        }

        public T[] GetComponents<T>()
            where T : IComponent
        {
            var components = Components.OfType<T>().ToArray();
            return components;
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            Components.Add(component);
        }
    }
}
