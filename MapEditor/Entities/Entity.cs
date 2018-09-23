using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Common;
using MapEditor.Components;
using Newtonsoft.Json;

namespace MapEditor.Entities
{
    public class Entity
    {
        // todo: replace TileId and EntityId with long, use static class to generate
        public Guid Id { get; }
        public List<IComponent> Components { get; set; }

        public Entity()
        {
            Id = Guid.NewGuid();
            Components = new List<IComponent>();
        }

        public T GetComponent<T>()
            where T : IComponent
        {
            //var component = Components.OfType<T>().SingleOrDefault();
            //if (component == null)
            //{
            //    throw new Exception($"Component {typeof(T)} does not exist in dictionary");
            //}
            var component = Components.OfType<T>().LastOrDefault();
            return component;
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            if (!Components.Any(x => x is T))
                Components.Add(component);
        }
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
