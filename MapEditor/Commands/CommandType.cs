namespace MapEditor.Commands
{
    public enum CommandType
    {
        None = 0,
        Undo,
        Redo,

        Move,
        Stop,
        RenderSelection,
        SelectUnits,
        AddUnit,

        CreateMap,
        PlaceTile,

        CreateCamera,
        MoveCamera
    }
}
