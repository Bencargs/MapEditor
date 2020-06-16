using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Components;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MapEngine.Handlers
{
    public class UnitHandler
    {
        private int _unitIndex;
        private readonly TextureHandler _textures;
        private readonly MovementHandler _movementHandler;

        private Dictionary<int, Entity> _units = new Dictionary<int, Entity>();

        public UnitHandler(TextureHandler textures, MovementHandler movementHandler)
        {
            _textures = textures;
            _movementHandler = movementHandler;
        }

        public void Init(string filename)
        {
            _textures.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures");

            var unit = ResourceLoader.LoadUnit(filename);
            _units.Add(_unitIndex++, unit);
        }

        public void Update()
        {
            foreach (var unit in _units.Values)
            {
                var command = new MovementCommand
                {
                    Entity = unit,
                    MovementMode = MovementMode.Seek,
                    Destination = new Vector2(50, 50)
                };

                _movementHandler.Handle(command);
            }
        }

        public void Render(IGraphics graphics)
        {
            foreach (var unit in _units.Values)
            {
                var location = unit.GetComponent<LocationComponent>().Location;
                var textureId = unit.GetComponent<ImageComponent>().TextureId;
                if (_textures.TryGetTexture(textureId, out var texture))
                {
                    var area = texture.Area(location);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }
    }
}
