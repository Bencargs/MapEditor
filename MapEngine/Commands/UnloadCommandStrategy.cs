using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using System.Numerics;
using Common;
using MapEngine.Entities.Components;
using MapEngine.Handlers.InputHandler;

namespace MapEngine.Commands
{
    public class UnloadCommandStrategy : ICommandStrategy
    {
        public InputState.Command CommandType => InputState.Command.Unload;

        public bool IsApplicable(Entity entity)
        {
            var state = entity.GetComponent<StateComponent>();
            if (!state?.CanTransition(State.Unloading) ?? false)
                return false;
            
            // Any entity with a cargo hold, that has loaded cargo
            return entity.GetComponents<CargoComponent>()
                .Any(x => x.Content.Any());
        }

        public ICommand CreateCommand(Vector2 location, List<Entity> entities)
        {
            var command = new UnloadCommand
            {
                Destination = location,
                Entities = entities
            };

            return command;
        }
    }
}