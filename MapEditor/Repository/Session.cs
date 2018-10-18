using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapEditor.Commands;
using MapEditor.Common;
using MapEditor.Components;
using MapEditor.Engine;
using MapEditor.Entities;

namespace MapEditor.Repository
{
    public interface ISession : IHandleCommand
    {
        void Init();

        Terrain GetTerrain(Guid id);
        Dictionary<Guid, Terrain> GetTerrains();

        Tile GetTile(Point point);
        Tile[,] GetTiles();
        Tile[,] GetTiles(Rectangle area);

        Entity GetUnit(Guid id);
        List<Entity> GetUnits(Point point);
        Dictionary<Guid, List<Entity>> GetUnits();
        IEnumerable<Entity> GetUnits(Rectangle area);

        IEnumerable<T> GetComponent<T>(Point point)
            where T : IComponent;
        IEnumerable<T> GetComponent<T>(Rectangle area)
            where T : IComponent;
        IEnumerable<T> GetComponent<T>(Vector2 path)
            where T : IComponent;

        Rectangle GetViewport();
        MapSettings GetMapSettings();
        UserInterface GetUserInterface();
    }

    public class Session : ISession
    {
        private readonly MessageHub _messageHub;
        private int Width { get; set; }
        private int Height { get; set; }
        private bool ShowGrid { get; set; }
        private bool ShowTerrain { get; set; }
        private Tile[,] Tiles { get; set; }
        private Dictionary<Guid, Terrain> Terrains { get; } = new Dictionary<Guid, Terrain>();
        private Dictionary<Guid, List<Entity>> Units { get; } = new Dictionary<Guid, List<Entity>>();
        private Rectangle Viewport { get; set; }
        private UserInterface UserInterface { get; set; } = new UserInterface();

        public Session(MessageHub messageHub)
        {
            _messageHub = messageHub;
        }

        public void Init()
        {
            _messageHub.Subscribe(this, CommandType.CreateMap);
            _messageHub.Subscribe(this, CommandType.CreateCamera);
            _messageHub.Subscribe(this, CommandType.RenderSelection);
        }

        public void Handle(ICommand command)
        {
            switch (command)
            {
                case CreateMapCommand c:
                    Width = c.MapSettings.Width;
                    Height = c.MapSettings.Height;
                    ShowGrid = c.MapSettings.ShowGrid;
                    ShowTerrain = c.MapSettings.ShowTerrain;
                    Tiles = new Tile[Width, Height];
                    break;
                case AddUnitCommand c:
                    var tile = GetTile(c.Point);
                    Units[tile.Id].Add(c.Unit);
                    break;
                case CreateCameraCommand c:
                    Viewport = c.Viewport;
                    break;
                case RenderSelectionCommand c:
                    UserInterface.Selection = c.Area;
                    break;
            }
        }

        public void Undo(ICommand command)
        {
            throw new NotImplementedException();
        }

        private Point ScreenToTile(Point point)
        {
            var viewport = GetViewport();
            var tileX = point.X * Width / viewport.Width;
            var tileY = point.Y * Height / viewport.Height;

            //todo: find a way to do this without using the above function
            if (tileX == 0)
                tileX = 1;
            else if (tileX >= Width - 1)
                tileX = Width - 1;

            if (tileY == 0)
                tileY = 1;
            else if (tileY >= Height - 1)
                tileY = Height - 1;

            return new Point(tileX, tileY);
        }

        public Terrain GetTerrain(Guid id)
        {
            return Terrains[id];
        }

        public Dictionary<Guid, Terrain> GetTerrains()
        {
            return Terrains;
        }

        public Tile GetTile(Point point)
        {
            var mapTile = ScreenToTile(point);

            return Tiles[mapTile.X, mapTile.Y];
        }

        public Tile[,] GetTiles()
        {
            return Tiles;
        }

        public Tile[,] GetTiles(Rectangle area)
        {
            var topPoint = ScreenToTile(new Point(area.Left, area.Top));
            var endPoint = ScreenToTile(new Point(area.Right, area.Bottom));

            var xRange = endPoint.X - topPoint.X;
            var yRange = endPoint.Y - topPoint.Y;
            var retVal = new Tile[xRange, yRange];

            var innerX = 0;
            for (var x = topPoint.X; x < endPoint.X; x++)
            {
                var innerY = 0;
                for (var y = topPoint.Y; y < endPoint.Y; y++)
                {
                    retVal[innerX, innerY] = Tiles[x, y];
                    innerY++;
                }
                innerX++;
            }
            return retVal;
        }

        public Entity GetUnit(Guid id)
        {
            return Units.Values.SelectMany(ut => ut).FirstOrDefault(u => u.Id == id);
        }

        public List<Entity> GetUnits(Point point)
        {
            var tile = GetTile(point);
            if (Units.TryGetValue(tile.Id, out List<Entity> units))
            {
                return units;
            }
            Units[tile.Id] = new List<Entity>();
            return Units[tile.Id];
        }

        public Dictionary<Guid, List<Entity>> GetUnits()
        {
            return Units;
        }

        public IEnumerable<Entity> GetUnits(Rectangle area)
        {
            var tiles = GetTiles(area);
            foreach (var t in tiles)
            {
                if (t == null)
                    continue;

                if (Units.TryGetValue(t.Id, out List<Entity> units))
                {
                    foreach (var u in units)
                    {
                        yield return u;
                    }
                }
                else
                {
                    Units[t.Id] = new List<Entity>();
                }
            }
        }

        public IEnumerable<T> GetComponent<T>(Point point) where T : IComponent
        {
            var units = GetUnits(point);
            foreach (var u in units)
            {
                var component = u.GetComponent<T>();
                if (component != null)
                    yield return component;
            }
        }

        public IEnumerable<T> GetComponent<T>(Rectangle area) where T : IComponent
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetComponent<T>(Vector2 path) where T : IComponent
        {
            throw new NotImplementedException();
        }

        public Rectangle GetViewport()
        {
            return Viewport;
        }

        public MapSettings GetMapSettings()
        {
            return new MapSettings
            {
                Width = Width,
                Height = Height,
                ShowGrid = ShowGrid,
                ShowTerrain = ShowTerrain
            };
        }

        public UserInterface GetUserInterface()
        {
            return UserInterface;
        }
    }
}
