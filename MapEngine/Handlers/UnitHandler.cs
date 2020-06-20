using Common;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.ResourceLoading;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Handlers
{
    public class UnitHandler
    {
        private int _unitIndex;
        private readonly TextureHandler _textures;
        private readonly MovementHandler _movementHandler;
        private readonly Dictionary<int, Entity> _units = new Dictionary<int, Entity>();

        public UnitHandler(TextureHandler textures, MovementHandler movementHandler)
        {
            _textures = textures;
            _movementHandler = movementHandler;
        }

        public void Init(string unitsFilepath, string mapFilepath)
        {
            // todo: refactor this to: 
            // unit = LoadUnitModel(); 
            // LoadTexture(unit.Texture);
            _textures.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures");

            var definitions = new Dictionary<string, Entity>();
            foreach (var file in Directory.GetFiles(unitsFilepath, "*.json"))
            {
                var unit = UnitLoader.LoadUnitDefinition(file);
                var type = unit.GetComponent<UnitComponent>();
                definitions.Add(type.UnitType, unit);
            }

            var units = UnitLoader.LoadUnits(mapFilepath, definitions);
            foreach (var unit in units)
            {
                _units.Add(_unitIndex++, unit);
            }
        }

        public void Update()
        {
            _movementHandler.Handle(_units[0]);
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
