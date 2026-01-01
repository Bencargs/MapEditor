using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common;
using Common.Collision;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers;
using MapEngine.Services.Map;

namespace MapEngine.Entities
{
    // todo: wrap entity in a Unit class instead of extension methods?
    public static class EntityEx
    {
        // todo: this may require an alliance lookup instead of simply comparing teams
        public static bool BelongsTo(this Entity entity, int team)
        {
            var entityTeam = entity.GetComponent<UnitComponent>()?.TeamId ?? 0;
            return entityTeam == team;
        }

        public static Vector2 Location(this Entity entity)
        {
            var location = entity.GetComponent<LocationComponent>();
            return location.Location;
        }

        public static int Elevation(this Entity entity)
        {
            var location = entity.GetComponent<LocationComponent>();
            return location.Elevation;
        }

        public static bool IsMoving(this Entity entity)
        {
            var movement = entity.GetComponent<MovementComponent>();
            return (movement?.Velocity.Length() ?? 0) > 0.1f;
        }

        public static Texture Texture(this Entity entity)
        {
            var imageComponent = entity.GetComponent<ImageComponent>();
            if (imageComponent == null)
                return null;

            var textureId = imageComponent.TextureId;
            return TextureFactory.TryGetTexture(textureId, out var texture) ? texture : null; // todo: null object?
        }

        public static Model Model(this Entity entity)
        {
            var modelComponent = entity.GetComponent<ModelComponent>();
            if (modelComponent == null)
                return null;
            
            return ModelFactory.TryGetModel(modelComponent.ModelId, out var model) ? model : null;
        }

        public static ICollider Hitbox(this Entity entity)
        {
            var colliderComponent = entity.GetComponent<CollisionComponent>();
            if (colliderComponent == null)
                return null; //  todo: null object?

            var sourceLocation = entity.Location();
            var collider = colliderComponent.GetCollider(sourceLocation);

            //var facingAngle = entity.GetComponent<LocationComponent>().FacingAngle;
            //var rotated = collider.Rotate(facingAngle);
            //return rotated;

            return collider;
        }

        public static bool IsNavigable(this Entity entity, Tile tile)
        {
            // todo: if a unit is larger than a tile, it will navigate through a space smaller than itself
            // get unit bounding box
            // get all contained tiles centered on this tile
            // if we fit within them all - then navigable

            var movementComponent = entity.GetComponent<MovementComponent>();
            if (movementComponent == null || tile is null) return false;

            var terrainTypes = movementComponent.Terrains;
            var tileGradient = tile.GetGradient();

            return terrainTypes.Contains(tile.Type) && tileGradient <= movementComponent.MaxGradient;
        }

        public static void ChangeState(this Entity entity, State state)
        {
            var stateComponent = entity.GetComponent<StateComponent>();
            stateComponent?.ChangeState(state);
        }

        public static void Complete(this Entity entity, State state)
        {
            var stateComponent = entity.GetComponent<StateComponent>();
            if (stateComponent?.CurrentState != state)
                return;
            
            stateComponent.ChangeState(State.Standby);
        }
        
        public static void Cancel(this Entity entity, State state)
        {
            var stateComponent = entity.GetComponent<StateComponent>();
            if (stateComponent?.CurrentState != state)
                return;
            
            stateComponent?.ChangeState(State.Stopping);
        }

        public static void ReplaceOrders(this Entity entity, IEnumerable<MoveOrder> orders)
        {
            var movementComponent = entity.GetComponent<MovementComponent>();
            movementComponent?.Destinations.Clear();
            movementComponent?.Destinations.Enqueue(orders);
        }

        public static void AddOrders(this Entity entity, IEnumerable<MoveOrder> orders)
        {
            var movementComponent = entity.GetComponent<MovementComponent>();
            movementComponent?.Destinations.Enqueue(orders);
        }
    }
}
