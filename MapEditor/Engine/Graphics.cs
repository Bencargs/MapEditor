using System.Drawing;
using System.Windows.Forms;
using MapEditor.Commands;
using MapEditor.Components;
using MapEditor.Repository;

namespace MapEditor.Engine
{
    /// <summary>
    /// todo: rename to renderer?
    /// </summary>
    public class WinFormGraphics : IGraphics, IHandleCommand
    {
        private readonly Image _bufferImage;
        private readonly Graphics _bufferGraphics;
        private readonly Graphics _graphics;
        private readonly ISession _session;
        private readonly Panel _window;

        public WinFormGraphics(MessageHub messageHub, ISession session, Panel window)
        {
            _session = session;
            _window = window;
            Width = window.Width;
            Height = window.Height;

            _bufferImage = new Bitmap(Width, Height);
            _bufferGraphics = Graphics.FromImage(_bufferImage);
            _graphics = window.CreateGraphics();

            messageHub.Subscribe(this, CommandType.RenderSelection);
            messageHub.Subscribe(this, CommandType.PlaceTile);
            messageHub.Subscribe(this, CommandType.AddUnit);
        }

        public int Width { get; }
        public int Height { get; }

        public void DrawLines(Color color, Point[] points)
        {
            var pen = new Pen(color, 1);
            _bufferGraphics.DrawLines(pen, points);
        }

        public void DrawImage(Image image, Rectangle area)
        {
            _bufferGraphics.DrawImageUnscaled(image, area);
        }

        public void Render()
        {
            //throw new System.NotImplementedException();
            //DrawRectangle(new SolidBrush(Color.FromArgb(255, 0, 190, 0)), new Rectangle(5, 5, 200, 200));
            //var tiles = _session.GetTiles();

            // todo: 
            // GetTiles(viewport, isUpdated: true)
            // GetUnits(viewport, isUpdated: true)
            // GetSelections(
            // todo:
            // OnUpdate: Scene.Update()
            // OnRender: _graphics.DrawImage(Scene.Image, viewpoert)

            _graphics.DrawImage(_bufferImage, new Rectangle(0, 0, Width, Height));
        }

        //Tech debt
        public void FillRectangle(Brush brush, Rectangle area)
        {
            _bufferGraphics.FillRectangle(brush, area);
        }

        public void DrawRectangle(Brush brush, Rectangle area)
        {
            _bufferGraphics.DrawRectangle(new Pen(brush), area);
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            _window?.Dispose();
            _bufferGraphics?.Dispose();
            _bufferImage?.Dispose();
        }

        public void Handle(ICommand command)
        {
            switch (command)
            {
                case RenderSelectionCommand c:
                    // todo: change signiture to only accept unit, position should be concern of the unit
                    DrawRectangle(new SolidBrush(Color.FromArgb(255, 0, 128, 0)), new Rectangle(c.Area.X -1, c.Area.Y - 1, c.Area.Width - 1, c.Area.Height - 1));
                    DrawRectangle(new SolidBrush(Color.FromArgb(255, 0, 190, 0)), c.Area);
                    break;
                case PlaceTileCommand c:
                    var tile = _session.GetTile(c.Point);
                    var terrain = _session.GetTerrain(tile.TerrainIndex);
                    var area = new Rectangle(tile.X, tile.Y, terrain.Width, terrain.Height);
                    DrawImage(c.Terrain.Image, area);
                    break;
                case AddUnitCommand c:
                    var unitImage = c.Unit.GetComponent<ImageComponent>().Image;
                    var unitArea = new Rectangle(c.Point, new Size(unitImage.Width, unitImage.Height));
                    DrawImage(unitImage, unitArea);
                    break;
            }
        }

        public void Undo(ICommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}
