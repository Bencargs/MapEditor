using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapEditor.Engine;

namespace MapEditor.Entities
{
    public enum ComponentType
    {
        Empty = 0,
        RenderImage,
        //RenderAnimation,
        Position,
        Selection,
        Movement,
        Collision,
    }

    // todo: merge into unit controller?
    public class ComponentController
    {
        // todo: modify to support multiple handlers for each componentType ?
        private Dictionary<ComponentType, IComponent> Components { get; } 
            = new Dictionary<ComponentType, IComponent>();

        public IComponent GetComponent(ComponentType type)
        {
            if (Components.TryGetValue(type, out IComponent component))
            {
                return component;
            }
            throw new Exception($"Component {type} does not exist in dictionary");
        }

        public void AddComponent(IComponent component)
        {
            if (!Components.ContainsKey(component.Type))
            {
                Components.Add(component.Type, component);
            }
        }
    }


    public interface IComponent
    {
        ComponentType Type { get; }
    }

    public interface IComponentHandler
    {
        void Handle(Entity entity);
    }

    public class PositionComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Position;
        public int X { get; set; }
        public int Y { get; set; }
        public List<TerrainType> TerrainTypes { get; set; }
    }

    public class ImageComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.RenderImage;
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
        public void Handle(Entity entity)
        {
            // Check pre-requisite components - in this case position is required for movement
            if (entity.Components.All(x => x.Type != ComponentType.Position))
                return;

            // update entities position, based on:
            // GameTimeElapsed - stored in Game.. passed via update command.. sent into Handle?
            // destination - updated via command
            // speed - data stored in MovementComponent
            // direction - data stored in MovementComponent
            // Inertia / Mass - data stored in .. MassComponent? UnitData?
            // TurnRate - data stored in MovementComponent? UnitData?
            // Path - set via pathfinding system - another pre-requisite?
            // Valid Terrain Types - data stored in MovementComponent? is this a pathfinding concern? cross cutting?

            throw new NotImplementedException();
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

        public void Handle(Entity entity)
        {
            // Does ordering of components matter? what if we update movement then render,
            // versus rendering then updating movement? how to we ensure consistency when 
            // components can be added or removed? sort by ComponentType in component handler?

            // Pass in relevant component data
            // todo: have each handler declare which component datas it requires passed to it?
            // how to validate handlers supported given entities components?

            // todo: refactor / remove ComponentController (extension class?)
            // Entity.GetComponent(ComponentType) ?
            var componentData = (ImageComponent) entity.Components.FirstOrDefault(x => x.Type == ComponentType.RenderImage);
            
            // todo: pre-requisite on Position OR Area - how to handle boolean requisites?

            //_graphics.DrawImage(componentData.Image, );

            throw new NotImplementedException();
        }
    }
}
