using Common;
using System.Numerics;

namespace MapEngine
{
    public static class Mouse
    {
        public static Vector2 Location { get; private set; }
        public static ButtonState State { get; private set; }

        public enum ButtonState
        {
            None = 0,
            LeftReleased = 1,
            LeftPressed = 2,
            RightReleased = 3,
            RightPressed = 4
        }

        public static void SetState(Vector2 location, ButtonState button)
        {
            Location = location;
            State = button;
        }
    }
}
