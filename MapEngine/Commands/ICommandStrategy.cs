using System.Collections.Generic;
using Common.Entities;
using System.Numerics;
using MapEngine.Handlers.InputHandler;

namespace MapEngine.Commands
{
    public interface ICommandStrategy
    {
        InputState.Command CommandType { get; }
        
        bool IsApplicable(Entity entity);
        
        ICommand CreateCommand(Vector2 location, List<Entity> entities);
    }
}