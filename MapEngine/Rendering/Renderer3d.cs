using System;
using System.Collections.Generic;
using System.Numerics;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Handlers;
using SoftEngine;

namespace MapEngine.Rendering
{
    public class Renderer3d
        : IRenderer
        , IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        // todo: this is pretty sus - should come from a game settings class
        private readonly Device _3dEngine = new Device(new WpfImage(768, 512));
        //private readonly Device _3dEngine = new Device(new WpfImage(640, 480));
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly SensorHandler _sensorHandler;

        public Renderer3d(SensorHandler sensorHandler)
        {
            _sensorHandler = sensorHandler;
        }

        public void DrawLayer(Rectangle viewport, IGraphics graphics)
        {
            foreach (var entity in _entities)
            {
                var location = entity.GetComponent<LocationComponent>();
                if (!viewport.Contains(location.Location))
                    continue;

                var isDetected = _sensorHandler.IsDetected(Constants.PlayerTeam, entity);
                if (!entity.BelongsTo(Constants.PlayerTeam) && !isDetected)
                    continue;

                var model = entity.Model();
                model.Location = new Vector3(0, 0, 2);

                // Note: the tiny offset on the Y axis prevents clipping through the plane
                var radians = (Math.PI / 180) * location.FacingAngle;
                model.Rotation = new Vector3(0, 0.001f, (float)radians);

                var texture = entity.Texture();
                var render = _3dEngine.Render(model, texture);
                var area = render.Area(location.Location);
                area.Translate(viewport.X, viewport.Y);

                graphics.DrawBytes(render.Buffer, area);
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.Texture() == null || entity.Model() == null)
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
