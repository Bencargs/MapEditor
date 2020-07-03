using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.ResourceLoading;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Handlers
{
    public class UnitHandler 
        : IHandleCommand<CreateEntityCommand>
    {
        private int _unitIndex;
        private readonly TextureHandler _textures;
        private readonly MovementHandler _movementHandler;
        private readonly Dictionary<int, Entity> _units = new Dictionary<int, Entity>();
        private readonly Dictionary<string, Entity> _prototypes = new Dictionary<string, Entity>();

        public UnitHandler(TextureHandler textures, MovementHandler movementHandler)
        {
            _textures = textures;
            _movementHandler = movementHandler;
        }

        public void Initialise(string unitsFilepath, string mapFilename)
        {
            // todo: refactor this to: 
            // unit = LoadUnitModel(); 
            // LoadTexture(unit.Texture);
            _textures.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures");

            foreach (var file in Directory.GetFiles(unitsFilepath, "*.json"))
            {
                var unit = UnitLoader.LoadUnitDefinition(file);
                var type = unit.GetComponent<UnitComponent>();
                _prototypes.Add(type.UnitType, unit);
            }

            var units = UnitLoader.LoadUnits(mapFilename, _prototypes);
            foreach (var unit in units)
            {
                _units.Add(_unitIndex++, unit);
            }
        }

        public void Update()
        {
            foreach (var (_, unit) in _units)
            {
                _movementHandler.Handle(unit);
            }
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            foreach (var unit in _units.Values)
            {
                var location = unit.GetComponent<LocationComponent>().Location;
                var textureId = unit.GetComponent<ImageComponent>().TextureId;
                if (_textures.TryGetTexture(textureId, out var texture))
                {
                    var area = texture.Area(location);
                    area.Translate(viewport.X, viewport.Y);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var unit = command.Entity.GetComponent<UnitComponent>();
            if (unit == null)
                return;

            _units.Add(_unitIndex++, command.Entity);
        }
    }
}
