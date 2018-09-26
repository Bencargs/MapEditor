using System.Drawing;
using MapEditor.Common;
using Newtonsoft.Json;

namespace MapEditor.Handlers.CollisionHandler
{
    [JsonConverter(typeof(EntityEx.ColliderCreationConverter))]
    public interface ICollider
    {
        ColliderType Type { get; }
        Point Position { get; set; }
        bool IsCollided(ICollider collider);
    }
}
