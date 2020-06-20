using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class ImageComponent : IComponent
    {
        public ComponentType Type => ComponentType.Image;
        public string TextureId { get; set; }

        public IComponent Clone()
        {
            return new ImageComponent
            {
                TextureId = TextureId
            };
        }
    }
}
