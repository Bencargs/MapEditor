using System;
using System.Drawing;
using Newtonsoft.Json;

namespace MapEditor.Components
{
    public class ImageComponent : IComponent
    {
        public ComponentType Type { get; set; } = ComponentType.Image;
        public Guid Id { get; set; }
        [JsonIgnore]
        public Bitmap Image { get; set; }
    }
}
