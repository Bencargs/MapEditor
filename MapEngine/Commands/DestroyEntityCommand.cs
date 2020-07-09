using Common.Entities;

namespace MapEngine.Commands
{
    public class DestroyEntityCommand : ICommand
    {
        public Entity Entity { get; set; }
    }
}
