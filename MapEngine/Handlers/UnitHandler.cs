using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Handlers
{
    public class UnitHandler 
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly MessageHub _messageHub;
        private readonly MovementHandler _movementHandler;
        private readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity>();
        private readonly Dictionary<string, Entity> _prototypes = new Dictionary<string, Entity>();

        public UnitHandler(MessageHub messageHub, MovementHandler movementHandler)
        {
            _messageHub = messageHub;
            _movementHandler = movementHandler;
        }

        public void Initialise(string unitsFilepath, string mapFilename)
        {
            // todo: refactor this to: 
            // unit = LoadUnitModel(); 
            // LoadTexture(unit.Texture);
            TextureFactory.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures");

            foreach (var file in Directory.GetFiles(unitsFilepath, "*.json"))
            {
                var unit = UnitLoader.LoadUnitDefinition(file);
                var type = unit.GetComponent<UnitComponent>();
                _prototypes.Add(type.UnitType, unit);
            }

            var units = UnitLoader.LoadUnits(mapFilename, _prototypes);
            foreach (var unit in units)
            {
                _messageHub.Post(new CreateEntityCommand { Entity = unit });
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            _entities.Add(command.Entity.Id, command.Entity);
        }

        public void Update()
        {
            _movementHandler.Update();
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            foreach (var unit in _entities.Values)
            {
                var location = unit.GetComponent<LocationComponent>().Location;
                var textureId = unit.GetComponent<ImageComponent>().TextureId;
                if (TextureFactory.TryGetTexture(textureId, out var texture))
                {
                    var area = texture.Area(location);
                    area.Translate(viewport.X, viewport.Y);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }

        public void Handle(DestroyEntityCommand command)
        {
            if (_entities.ContainsKey(command.Entity.Id))
            {
                _entities.Remove(command.Entity.Id);
            }
        }
    }
}
