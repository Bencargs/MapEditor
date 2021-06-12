using Common.Entities;

namespace MapEngine.Entities.Components
{
    // todo: multiple responsibilities?
    public class UnitComponent : IComponent
    {
        public ComponentType Type => ComponentType.Unit;
        public string UnitType { get; set; }
        public int TeamId { get; set; }

        public IComponent Clone()
        {
            return new UnitComponent
            {
                UnitType = UnitType,
                TeamId = TeamId
            };
        }
    }
}
