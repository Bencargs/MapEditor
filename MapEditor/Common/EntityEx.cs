using System;
using MapEditor.Commands;
using MapEditor.Components;
using MapEditor.Entities;
using MapEditor.Handlers;
using MapEditor.Handlers.CollisionHandler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapEditor.Common
{
    public static class EntityEx
    {
        public static T Clone<T>(this T source)
        {
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            var serialized = JsonConvert.SerializeObject(source, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            return JsonConvert.DeserializeObject<T>(serialized, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        public static Entity Clone(this Entity source)
        {
            var entity = new Entity();
            foreach (var c in source.Components)
            {
                entity.Components.Add(c.Clone());
            }
            var imageComponent = entity.GetComponent<ImageComponent>();
            if (imageComponent != null)
            {
                imageComponent.Image = source.GetComponent<ImageComponent>().Image;
            }
            return entity;
        }

        public abstract class JsonCreationConverter<T> : JsonConverter
        {
            public override bool CanWrite => false;

            protected abstract T Create(Type objectType, JObject jsonObject);

            public override bool CanConvert(Type objectType)
            {
                return typeof(T).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType,
                object exisitngValue, JsonSerializer serializer)
            {
                var jsonObject = JObject.Load(reader);
                var target = Create(objectType, jsonObject);
                serializer.Populate(jsonObject.CreateReader(), target);
                return target;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class ComponentCreationConverter : JsonCreationConverter<IComponent>
        {
            protected override IComponent Create(Type objectType, JObject jsonObject)
            {
                var typeName = jsonObject["Type"].ToObject<ComponentType>();
                switch (typeName)
                {
                    case ComponentType.Animation:
                        throw new NotImplementedException();
                    case ComponentType.Collision:
                        return new CollisionComponent();
                    case ComponentType.Image:
                        return new ImageComponent();
                    case ComponentType.Movement:
                        return new MovementComponent();
                    case ComponentType.Pathing:
                        return new PathingComponent();
                    case ComponentType.Physics:
                        return new PhysicsComponent();
                    case ComponentType.Position:
                        return new PositionComponent();
                    case ComponentType.Unit:
                        return new UnitComponent();
                }
                return null;
            }
        }

        public class ColliderCreationConverter : JsonCreationConverter<ICollider>
        {
            protected override ICollider Create(Type objectType, JObject jsonObject)
            {
                var typeName = jsonObject["Type"].ToObject<ColliderType>();
                switch (typeName)
                {
                    case ColliderType.BoundingCircle:
                        return new BoundingCircle();
                    case ColliderType.BoundingBox:
                        return new BoundingBox();
                }
                return null;
            }
        }
    }
}
