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
            var component = Components.OfType<T>().LastOrDefault();
            return component;
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            if (!Components.Any(x => x is T))
                Components.Add(component);
        }
    }
}
