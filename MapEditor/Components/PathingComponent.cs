using System.Collections.Generic;
using MapEditor.Engine;

namespace MapEditor.Components
{
    public class PathingComponent : IComponent
    {
        public List<ITarget> Destinations = new List<ITarget>();
    }
}
