using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapEditor.Engine;

namespace MapEditor.Entities
{
    //public enum ComponentType
    //{
    //    Empty = 0,
    //    RenderImage,
    //    //RenderAnimation,
    //    Position,
    //    Selection,
    //    Movement,
    //    Collision,
    //}


    public class Entity
    {
        // todo: replace TileId and EntityId with long, use static class to generate
        public Guid Id { get; } = Guid.NewGuid();
        public List<IComponent> Components { get; set; }

        public T GetComponent<T>()
            where T : IComponent
        {
            var component = Components.OfType<T>().SingleOrDefault();
            if (component == null)
            {
                throw new Exception($"Component {typeof(T)} does not exist in dictionary");
            }
            return component;
        }

        public void AddComponent<T>(T component)
            where T : IComponent
        {
            if (!Components.Any(x => x is T))
                Components.Add(component);
        }
    }

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

    // https://gamedevelopment.tutsplus.com/series/understanding-steering-behaviors--gamedev-12732
    public enum MovementMode
    {
        Seek = 0,
        Follow,
        Intercept,
        Evade,
        Roam,
    }

    // todo: temporary - replace when using a real framework
    public class Vector2
    {
        public int X;
        public int Y;

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return null;
        }

        public static Vector2 operator /(Vector2 a, float source)
        {
            return null;
        }

        public Vector2 Truncate(float value)
        {
            return null;
        }

        public float Angle()
        {
            return -1;
        }
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

        public Vector2 Steering { get; set; }
        public Vector2 Velocity { get; set; }
        public float MaxVelocity { get; set; }
        public float Force { get; set; }
        public float SlowRadius { get; set; }
    }

    public class ImageComponent : IComponent
    {
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

    public class MovementHandler : IComponentHandler
    {
        // todo: ensure this is checked on loan, not during game runtime
        public IEnumerable<Type> RequiredComponents { get; } = new[]
        {
            typeof(PositionComponent),
            typeof(MovementComponent),
            typeof(PhysicsComponent),
            typeof(PathingComponent)
        };

        public void Handle(Entity entity, double elapsed)
        {
            // update entities position, based on:
            // GameTimeElapsed - stored in Game.. passed via update command.. sent into Handle?
            // destination - updated via command
            // speed - data stored in MovementComponent
            // direction - data stored in MovementComponent
            // Inertia / Mass - data stored in .. MassComponent? UnitData?
            // Path - set via pathfinding system - another pre-requisite?
            // Valid Terrain Types - data stored in MovementComponent? is this a pathfinding concern? cross cutting?

            // todo: if withing X of target, remove target from destinations

            var positionData = entity.GetComponent<PositionComponent>();
            var movementData = entity.GetComponent<MovementComponent>();
            var physicsData = entity.GetComponent<PhysicsComponent>();
            var pathingData = entity.GetComponent<PathingComponent>();
            if (!pathingData.Destinations.Any())
                // todo: set some kind of flag to indicate movement is not required
                return;

            switch (movementData.MovementMode)
            {
                case MovementMode.Seek:
                    movementData.Steering = Seek(pathingData, positionData, movementData, physicsData);
                    break;
                case MovementMode.Intercept:
                    movementData.Steering = Intercept()
                    break;
                case MovementMode.Follow:
                    break;
                case MovementMode.Evade:
                    break;
                case MovementMode.Roam:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Update(positionData, movementData);
        }

        private void Update(PositionComponent positionData, MovementComponent movementData)
        {
            // using Steering, Force, Mass
            // Set Velocity, considering MaxVelocity
            // Set Position

            ////then add it to velocity to get the new velocity
            //movement.Velocity = (movement.Velocity + movement.Steering).Truncate(movement.MaxVelocity);

            //position.Position += movement.Velocity;
        }

        private Vector2 Seek(PathingComponent pathing, 
                             PositionComponent position, 
                             MovementComponent movement, 
                             PhysicsComponent physics)
        {
            var target = pathing.Destinations.First();

            //subtract the position from the target to get the vector from the vehicles position to the target. 
            var distance = (target.Position - position.Position)/* / Data.MaxVelocity*/;

            //Normalize it then multiply by max speed to get the maximum velocity from your position to the target.
            var desiredVelocity = distance.Truncate(movement.MaxVelocity); // distance.Normalize()

            //subtract velocity from the desired velocity to get the force vector
            var steering = desiredVelocity - movement.Velocity;

            //if (distance <= movement.SlowRadius)
            //    desiredVelocity.ScaleBy(movement.MaxVelocity * distance / movement.SlowRadius;
            //else
            //    desiredVelocity.ScaleBy(movement.MaxVelocity);

            //divide the steeringForce by the mass(which makes it the acceleration), 
            steering = steering.Truncate(movement.Force) / physics.Mass;

            return steering;
        }

        private void Follow(ITarget target)
        {
            
        }

        private void Intercept(PathingComponent pathing, PositionComponent position, MovementComponent movement, PhysicsComponent physics)
        {
            var target = pathing.Destinations.First();

            var distance = target.Position - position.Position;

            var updatesNeeded = distance / movement.MaxVelocity;

            var targetVector = target.Velocity.
        }

        private void Evade(ITarget target)
        {
            
        }

        private void Roam(ITarget target)
        {
            
        }
    }

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
