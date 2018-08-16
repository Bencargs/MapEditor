using System.Drawing;
using MapEditor.Commands;
using MapEditor.Engine;

namespace MapEditor.Entities
{
    public abstract class Entity
    {
        public Point Position { get; set; }
        public Animation Animation { get; set; }

        public void Update(double elapsed)
        {
            // MessageHub, get messages
            var command = new StopCommand();
            ChangeState(command);
        }

        public void Render(IGraphics graphics)
        {
            //var image = _animation.GetImage();//use elapsed to calculate
            var image = Animation.Next();
            graphics.DrawImage(image, Position);
        }

        protected abstract Entity ChangeState(ICommand c);
    }

    public class IdleUnit : Entity
    {
        protected override Entity ChangeState(ICommand c)
        {
            if (c is MoveCommand)
                return new MovingUnit();

            return this;
        }
    }

    public class StoppingUnit : Entity
    {
        public double Countdown { get; set; }

        protected override Entity ChangeState(ICommand c)
        {
            if (c is MoveCommand)
                return new MovingUnit();

            return this;
        }
    }

    public class MovingUnit : Entity
    {
        public Point Destination { get; set; }

        protected override Entity ChangeState(ICommand c)
        {
            if (c is StopCommand)
                return new StoppingUnit();
            // if c == Move - change move directions (if shift move, append to command queue

            return this;
        }
    }
}
