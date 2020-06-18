using System;

namespace Common.Entities
{
    public interface IComponent
    {
        ComponentType Type { get; }

        IComponent Clone();
    }
}
