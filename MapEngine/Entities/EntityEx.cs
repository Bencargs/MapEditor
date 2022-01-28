using System.Collections.Generic;
using System.Numerics;
using Common.Collision;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers;

namespace MapEngine.Entities
{
    // todo: wrap entity in a Unit class instead of extension methods?
    public static class EntityEx
    {
        // todo: team enum / class?
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

        public static Texture Texture(this Entity entity)
        {
            var imageComponent = entity.GetComponent<ImageComponent>();
            if (imageComponent == null)
                return null;

            var textureId = imageComponent.TextureId;
            return TextureFactory.TryGetTexture(textureId, out var texture) ? texture : null; // todo: null object?
        }

        // todo: tryget?
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

            var sourceLocation = entity.GetComponent<LocationComponent>().Location;
            var collider = colliderComponent.GetCollider(sourceLocation);// yuck
            return collider;
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
