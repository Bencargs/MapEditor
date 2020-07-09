using Common;
using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Handlers;
using System.Collections.Generic;
using System.Numerics;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly MessageHub _messageHub;
        private readonly MapHandler _mapHandler;
        private readonly UnitHandler _unitHandler;
        private readonly CameraHandler _cameraHandler;

        public Scene(
            IGraphics graphics,
            MessageHub messageHub,
            MapHandler mapHandler, //todo - replace with a handler provider as this grows unweildy?
            UnitHandler unitHandler,
            CameraHandler cameraHandler)
        {
            _graphics = graphics;
            _messageHub = messageHub;
            _mapHandler = mapHandler;
            _unitHandler = unitHandler;
            _cameraHandler = cameraHandler;
        }

        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap1.json";
            _cameraHandler.Initialise(mapFilename);
            _mapHandler.Initialise(mapFilename);

            var unitsPath = @"C:\Source\MapEditor\MapEngine\Content\Units\";
            _unitHandler.Initialise(unitsPath, mapFilename);
        }

        public void Display()
        {
            Update();

            Render();
        }

        private bool _spawn = true;
        private void Update()
        {
            if (_spawn)
            {
                _messageHub.Post(new CreateEntityCommand
                {
                    Entity = new Entity
                    {
                        Id = 4, // todo: EntityFactory
                        Components = new List<IComponent>
                        {
                            new LocationComponent { Location = new Vector2(300, 300) },
                            //new DamageComponent { },
                            new CollisionComponent (new BoundingCircle { Radius = 10 } ),
                            new ImageComponent { TextureId = "projectile" },
                            new MovementComponent
                            {
                                Velocity = new Vector2(0, -10),
                                Steering = Vector2.Zero,
                                MaxVelocity = 10,
                                Mass = 1,
                                MaxForce = 1,
                                StopRadius = 1,
                                Destinations = new Queue<MoveOrder>(new[] 
                                {
                                    new MoveOrder
                                    {
                                        Destination = new Vector2(320, 50),
                                        MovementMode = MovementMode.Seek,
                                    }
                                })
                            },
                        }
                    }
                });
                _spawn = false;
            }

            _messageHub.Notify();
            _cameraHandler.Update();
            _unitHandler.Update();
        }

        private void Render()
        {
            _graphics.Clear();

            var viewport = _cameraHandler.GetViewport();
            _mapHandler.Render(viewport, _graphics);
            _unitHandler.Render(viewport, _graphics);

            _graphics.Render();
        }

        private void ProcessInput()
        {

        }
    }
}
