using System.Numerics;
using Common;

namespace MapEngine.Handlers.InputHandler;

public interface IInterface
{
    Rectangle Area { get; }
    
    // null if non-interactive area
    Vector2? ScreenToWorld(Vector2 point);
}