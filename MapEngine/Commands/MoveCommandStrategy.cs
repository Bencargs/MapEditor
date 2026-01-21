using System.Collections.Generic;
using System.Numerics;
using Common;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Handlers;
using MapEngine.Handlers.InputHandler;

namespace MapEngine.Commands;

public class MoveCommandStrategy : ICommandStrategy
{
    public InputState.Command CommandType => InputState.Command.Move;
    
    public bool IsApplicable(Entity entity)
    {
        var state = entity.GetComponent<StateComponent>();
        if (!state?.CanTransition(State.Moving) ?? false)
            return false;

        return entity.GetComponent<MovementComponent>() != null;
    }

    public ICommand CreateCommand(Vector2 location, List<Entity> entities)
    {
        var moveCommand = new MoveCommand
        {
            Entities = entities,
            Destination = location,
            MovementMode = MovementMode.Seek,
            Queue = false, // todo: check of shift is down
        };
        return moveCommand;
    }
}