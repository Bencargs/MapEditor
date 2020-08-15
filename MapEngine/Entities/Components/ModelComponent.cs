using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class ModelComponent : IComponent
    {
        public ComponentType Type => ComponentType.Model;

        public string ModelId { get; set; }

        public IComponent Clone()
        {
            return new ModelComponent
            {
                ModelId = ModelId
            };
        }
    }
}
