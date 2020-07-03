using Common.Entities;

namespace MapEngine.Commands
{
    public class CreateEntityCommand : ICommand
    {
        public Entity Entity { get; set; }
    }
}
