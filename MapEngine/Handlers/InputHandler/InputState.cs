using System.Collections.Generic;
using System.Numerics;
using Common.Entities;

namespace MapEngine.Handlers.InputHandler;

public class InputState
{
    public Vector2 Location { get; set; }
    public Entity? HoveredEntity { get; set; }
    public Vector2? SelectionStart { get; set; }
    public Command CurrentCommand = Command.None;
    public readonly List<Entity> SelectedEntities = new List<Entity>();
        
    public bool IsTyping { get; set; }
    public string TextInput { get; set; }
        
    public enum Command
    {
        None,
        Stop,
        Move,
        Attack,
        Load,
        Unload,
        Guard,
        Patrol
    }
}