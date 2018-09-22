using MapEditor.Components;
using MapEditor.Entities;
using Newtonsoft.Json;

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
    }
}
