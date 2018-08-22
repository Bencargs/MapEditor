using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Commands;
using MapEditor.Engine;

namespace MapEditor.Entities
{

    public class Entity
    {
        // todo: replace TileId and EntityId with long, use static class to generate
        public Guid Id { get; } = Guid.NewGuid();
        public List<IComponent> Components { get; set; }
    }

    // below: Handling units as Finite State Machines
    // replaced with an Entity Component System, 
    // FSM can be retrofitted as a component later

    //public abstract class Entity
    //{
    //    public Point Position { get; set; }
    //    public Animation Animation { get; set; }

    //    public void Update(double elapsed)
    //    {
    //        // MessageHub, get messages
    //        var command = new StopCommand();
    //        ChangeState(command);
    //    }

    //    public void Render(IGraphics graphics)
    //    {
    //        //var image = _animation.GetImage();//use elapsed to calculate
    //        var image = Animation.Next();
    //        //graphics.DrawImage(image, Position);
    //    }

    //    protected abstract Entity ChangeState(ICommand c);
    //}

    //public class Unit : Entity
    //{
    //    //private UnitData UnitData { get; set; }
    //    //private List<IComponent> MovementComponents { get; set; }

    //    protected override Entity ChangeState(ICommand c)
    //    {
    //        if (c is MoveCommand)
    //        {
    //            //foreach (var mc in MovementComponents)
    //            //{
    //            //    mc.Handle(c);
    //            //}
    //        }

    //        return this;
    //    }
    //}

    //public class IdleUnit : Entity
    //{
    //    protected override Entity ChangeState(ICommand c)
    //    {
    //        if (c is MoveCommand)
    //            return new MovingUnit();

    //        return this;
    //    }
    //}

    //public class StoppingUnit : Entity
    //{
    //    public double Countdown { get; set; }

    //    protected override Entity ChangeState(ICommand c)
    //    {
    //        if (c is MoveCommand)
    //            return new MovingUnit();

    //        return this;
    //    }
    //}

    //public class MovingUnit : Entity
    //{
    //    public Point Destination { get; set; }

    //    protected override Entity ChangeState(ICommand c)
    //    {
    //        if (c is StopCommand)
    //            return new StoppingUnit();
    //        // if c == Move - change move directions (if shift move, append to command queue

    //        return this;
    //    }
    //}
}
