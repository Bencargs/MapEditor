using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Common;
using MapEditor.Components;
using Newtonsoft.Json;

namespace MapEditor.Entities
{
    public interface IComponent
    {
    }

    public interface IComponentHandler
    {
        IEnumerable<Type> RequiredComponents { get; }

        void Handle(Entity entity, double elapsed);
    }

    public class PositionComponent : IComponent
    {
        public Vector2 Position { get; set; }

        // todo: where neccessary:
        //Set the facingAngle (used to draw the image, in radians) to velocity
        //FacingAngle = Data.Velocity.Angle();
    }

    // todo: rename this
    // any object that can be a movement destination must implement this interface
    // eg: point on make is X & Y all other propertys are null
    // follow a unit - get these properties from the unit
    public interface ITarget
    {
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        float MaxVelocity { get; set; }
        float Mass { get; set; }   //todo: should this be a seperate component?
    }

    public class PathingComponent : IComponent
    {
        public List<ITarget> Destinations = new List<ITarget>();
    }

    public class PhysicsComponent : IComponent
    {
        public float Mass { get; set; }
    }

    public class MovementComponent : IComponent
    {
        // todo: rather than a single mode, it is possible to have multiple modes
        // eg- seek this target, while avoiding this area - replace with flag instead?
        public MovementMode MovementMode { get; set; }
        public List<TerrainType> TerrainTypes { get; set; }

        public Vector2 Velocity { get; set; }
        public float MaxVelocity { get; set; }
        public float Force { get; set; }
        public float SlowRadius { get; set; }
        public float StopRadius { get; set; }
    }

    public class ImageComponent : IComponent
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public Bitmap Image { get; set; }
    }

    //public class AnimationComponent : IComponent
    //{
    //    public ComponentType Type { get; } = ComponentType.RenderAnimation;
    //    public Animation Animation { get; set; }
    //}

    //public class PositionHandler : IComponentHandler
    //{
    //    public void Handle(Entity command)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    public class ImageRenderHandler : IComponentHandler
    {
        // todo: replace with an adbstraction class, seperate adding to draw queue and actually drawing
        private readonly Graphics _graphics;

        public ImageRenderHandler(Graphics graphics)
        {
            _graphics = graphics;
        }

        public IEnumerable<Type> RequiredComponents { get; }

        public void Handle(Entity entity, double elapsed)
        {
            // Does ordering of components matter? what if we update movement then render,
            // versus rendering then updating movement? how to we ensure consistency when 
            // components can be added or removed? sort by ComponentType in component handler?

            // Pass in relevant component data
            // todo: have each handler declare which component datas it requires passed to it?
            // how to validate handlers supported given entities components?

            // todo: refactor / remove ComponentController (extension class?)
            // Entity.GetComponent(ComponentType) ?
            //var componentData = (ImageComponent) entity.Components.FirstOrDefault(x => x.Type == ComponentType.RenderImage);
            
            // todo: pre-requisite on Position OR Area - how to handle boolean requisites?

            //_graphics.DrawImage(componentData.Image, );

            throw new NotImplementedException();
        }
    }
}
