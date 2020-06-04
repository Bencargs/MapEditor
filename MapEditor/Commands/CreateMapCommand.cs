using MapEditor.Engine;

namespace MapEditor.Commands
{
    public class CreateMapCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.CreateMap;
        public MapSettings MapSettings { get; set; }
    }
}
