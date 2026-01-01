using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;

namespace MapEngine.Handlers
{
    public class CargoHandler : 
        IHandleCommand<UnloadCommand>,
        IHandleCommand<CreateEntityCommand>,
        IHandleCommand<DestroyEntityCommand>
    {
        private readonly List<Entity> _entities = new List<Entity>();
        
        public void Update()
        {
            foreach (var entity in _entities)
            {
                var arrivedCargo = new List<Entity>();
                var cargoComponent = entity.GetComponent<CargoComponent>();
                foreach (var cargo in cargoComponent.Content)
                {
                    var cargoLocation = cargo.GetComponent<LocationComponent>();
                    if (cargoComponent.Destination == null || cargoLocation == null)
                        continue;

                    if (!HasArrived(cargoComponent.Destination.Value, cargoLocation.Location, cargoComponent.StopRadius))
                    {
                        var direction = (cargoComponent.Destination.Value - cargoLocation.Location).Normalize();
                        cargoLocation.Location += direction * cargoComponent.UnloadVelocity;
                    }
                    else // todo: validate is a valid unload point? eg. terrain?
                    {
                        arrivedCargo.Add(cargo);
                    }
                }
                
                foreach (var cargo in arrivedCargo)
                {
                    cargoComponent.Content.Remove(cargo);
                    cargo.Complete();
                }

                if (!cargoComponent.Content.Any())
                {
                    entity.Complete();
                }
            }
        }
        
        private static bool HasArrived(Vector2 location, Vector2 target, float stopRadius)
        {
            return Math.Abs(Vector2.Distance(location, target)) < stopRadius;
        }

        public void Handle(UnloadCommand command)
        {
            foreach (var entity in command.Entities)
            {
                var cargoComponent = entity.GetComponent<CargoComponent>();
                
                // Eg. factories may have static unload points, as opposed to the clicked destination
                cargoComponent.Destination = cargoComponent.UnloadPoint != null
                    ? entity.Location() - cargoComponent.UnloadPoint
                    : command.Destination;

                entity.ChangeState(State.Unloading);
            }
        }

        // todo: something that is aware of all entities should be a common component
        // todo: validating which entities are relevant should be a common handler method
        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.GetComponent<CargoComponent>() == null)
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}