using Common.Entities;

namespace MapEngine.Components
{
    class ImageComponent : IComponent
    {
        public ComponentType Type => ComponentType.Image;
        public string TextureId { get; set; }
    }
}
