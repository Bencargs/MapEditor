using System.Numerics;

namespace MapEngine.Handlers
{
    public class MoveOrder
    {
        public MovementMode MovementMode { get; set; }
        public Vector2 Destination { get; set; }

        public MoveOrder Clone()
        {
            return new MoveOrder
            {
                MovementMode = MovementMode,
                Destination = Destination
            };
        }
    }
}
