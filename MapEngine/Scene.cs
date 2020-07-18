﻿using Common;
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
        private readonly EntityHandler _unitHandler;
        private readonly CameraHandler _cameraHandler;

        public Scene(
            IGraphics graphics,
            MessageHub messageHub,
            MapHandler mapHandler, //todo - replace with a handler provider as this grows unweildy?
            EntityHandler unitHandler,
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
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap2.json";
            _cameraHandler.Initialise(mapFilename);
            _mapHandler.Initialise(mapFilename);

            var weaponsPath = @"C:\Source\MapEditor\MapEngine\Content\Weapons\";
            var unitsPath = @"C:\Source\MapEditor\MapEngine\Content\Units\";
            _unitHandler.Initialise(unitsPath, mapFilename, weaponsPath);
        }

        public void Display()
        {
            Update();

            Render();
        }

        private void Update()
        {
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
    }
}
