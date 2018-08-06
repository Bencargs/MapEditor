using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapEditor
{
    public partial class Form1 : Form
    {
        private int _cellWidth = 20;
        private int _cellHeight = 20;
        private readonly Graphics _graphics;
        private Pen _pen;
        private bool _updated = true;
        private TileForm _contextMenu;
        private Tile[,] _cells;

        public Form1()
        {
            InitializeComponent();
            _graphics = canvas.CreateGraphics();

            _cells = new Tile[canvas.Width / _cellWidth, canvas.Height / _cellHeight];
            for (var x = 0; x < canvas.Width / _cellWidth; x++)
            {
                for (var y = 0; y < canvas.Height / _cellHeight; y++)
                {
                    _cells[x, y] = new Tile();
                }
            }
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (_updated)
            {
                DrawGrid();
                canvas.Refresh();
                _updated = false;
            }
        }

        private void DrawGrid()
        {
            for (var x = 0; x < canvas.Width / _cellWidth; x++)
            {
                DrawLine(new Point(x * _cellWidth, 0), new Point(x * _cellWidth, canvas.Height));
            }
            for (var y = 0; y < canvas.Height / _cellWidth; y++)
            {
                DrawLine(new Point(0, y * _cellHeight), new Point(canvas.Width, y * _cellHeight));
            }
        }

        private void DrawLine(Point start, Point end)
        {
            var points = new[]
            {
                start,
                end
            };
            _pen = new Pen(Color.LightBlue, 1);
            _graphics.DrawLines(_pen, points);
        }

        private void canvas_Click(object sender, EventArgs e)
        {
            _contextMenu?.Close();
            if ((e as MouseEventArgs)?.Button == MouseButtons.Right)
            {
                var cell = GetCell();

                _contextMenu = new TileForm(cell);
                _contextMenu.Show();
                _contextMenu.SetDesktopLocation(Cursor.Position.X, Cursor.Position.Y);
            }
        }

        private Tile GetCell()
        {
            var point = canvas.PointToClient(Cursor.Position);

            var maxX = _cells.GetLength(0);
            var maxY = _cells.GetLength(1);

            var x = point.X * maxX / canvas.Width;
            var y = point.Y * maxY  / canvas.Height;

            x = x > maxX ? maxX : x;
            y = y > maxY ? maxY : y;

            return _cells[x, y];
        }
    }
}
