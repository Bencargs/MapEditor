using System;
using System.Collections.Generic;
using MapEditor.Entities;

namespace MapEditor.Handlers
{
    public interface IComponentHandler
    {
        IEnumerable<Type> RequiredComponents { get; }

        void Handle(Entity entity, double elapsed);
    }
}
