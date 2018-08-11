using MapEditor.Commands;

namespace MapEditor.Entities
{
    public abstract class Entity
    {
        public int X { get; set; }
        public int Y { get; set; }

        public void Update(double elapsed)
        {
            // MessageHub, get messages
            var command = new StopCommand();
            ChangeState(command);
        }

        protected abstract Entity ChangeState(ICommand c);
    }

    public class UnitController
    {
        // Create default of each unit type
        // Place at correct possition
        // Hold list of units (and active units)

        //.CreateInfantry
        //new IdleUnit() {a, b, c}
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
        public int DestinationX { get; set; }
        public int DestinationY { get; set; }

        protected override Entity ChangeState(ICommand c)
        {
            if (c is StopCommand)
                return new MovingUnit();
            // if c == Move - change move directions (if shift move, append to command queue

            return this;
        }
    }
}
