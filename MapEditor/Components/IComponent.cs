using MapEditor.Common;
using Newtonsoft.Json;

namespace MapEditor.Components
{
    [JsonConverter(typeof(EntityEx.ComponentCreationConverter))]
    public interface IComponent
    {
        ComponentType Type { get; }
    }
}
