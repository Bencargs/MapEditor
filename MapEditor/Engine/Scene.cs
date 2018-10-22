using System.Collections.Generic;
using System.Drawing;
using MapEditor.Commands;
using MapEditor.Components;
using MapEditor.Entities;
using MapEditor.Repository;

namespace MapEditor.Engine
{
    public class Scene
    {
        private readonly ISession _session;
        private readonly IGraphics _graphics;

        public Scene(ISession session, IGraphics graphics)
        {
            _session = session;
            _graphics = graphics;
        }

        public void Render()
        {
            _graphics.Render();

            var viewport = _session.GetViewport();

            var tiles = _session.GetTiles();//_session.GetTiles(viewport);

            var units = _session.GetUnits(viewport);

            var userInterface = _session.GetUserInterface();

            // todo: sort by Z value

            Clear();

            DrawTiles(tiles);

            DrawUnits(units);

            // GetFeatures
            // GetParticles

            DrawUserInterface(userInterface);

            _graphics.Render();
        }

        private void DrawUserInterface(UserInterface userInterface)
        {
            if (userInterface?.Selection != null)
            {
                var area = userInterface.Selection.Value;
                _graphics.DrawRectangle(new SolidBrush(Color.FromArgb(255, 0, 128, 0)),
                    new Rectangle(area.X - 1, area.Y - 1, area.Width - 1, area.Height - 1));
                _graphics.DrawRectangle(new SolidBrush(Color.FromArgb(255, 0, 190, 0)), area);
            }
        }

        private void DrawUnits(IEnumerable<Entity> units)
        {
            foreach (var unit in units)
            {
                var image = unit.GetComponent<ImageComponent>().Image;
                var position = unit.GetComponent<PositionComponent>().Position;
                var area = new Rectangle(position.X, position.Y, image.Width, image.Height);

                var unitComponent = unit.GetComponent<UnitComponent>();
                if (unitComponent.IsSelected)
                {
                    var selectionArea = new Rectangle(position.X + 5, position.Y + 15, unitComponent.SelectionRadius,
                        unitComponent.SelectionRadius);
                    _graphics.DrawCircle(Color.FromArgb(255, 0, 190, 0), selectionArea);
                }

                _graphics.DrawImage(image, area);
            }
        }

        private void DrawTiles(Tile[,] tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile == null)
                    continue;

                var terrain = _session.GetTerrain(tile.TerrainIndex);
                if (terrain.Image != null)
                {
                    var area = new Rectangle(tile.X, tile.Y, terrain.Width, terrain.Height);
                    _graphics.DrawImage(terrain.Image, area);
                }
            }
        }

        private void Clear()
        {
            _graphics.Clear();
        }
    }
}
