using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapEditor.Entities;
using Newtonsoft.Json;

namespace MapEditor
{
    public partial class Form1 : Form
    {
        private readonly UnitController _unitController;
        private TileForm _contextMenu;
        private Map _map;

        public Form1()
        {
            InitializeComponent();

            var graphics = new WinFormGraphics(canvas);
            _map = new Map(graphics, canvas.Width / 20, canvas.Height / 20)
                {
                    ShowGrid = gridChk.Checked
                };
            _map.Init();

            _unitController = new UnitController(graphics);
            //_unitController.CreateUnit(8, 8);

            var tile1 = new Bitmap(@"C:\Source\MapEditor\MapEditor\Map\Grass.png");
            tile1.MakeTransparent(Color.Fuchsia);
            button2.Tag = new Tile(0, 0, 20, Terrain.Land) { Image = tile1};

            var timer = new Timer();
            timer.Tick += Update;
            timer.Interval = 100; // in miliseconds
            timer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            _map.Update();
            _unitController.Update();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            _map.Render();
            _unitController.Render();
        }

        private void canvas_Click(object sender, EventArgs e)
        {
            _contextMenu?.Close();
            if ((e as MouseEventArgs)?.Button == MouseButtons.Right)
            {
                var point = ((MouseEventArgs) e).Location;// canvas.PointToClient(Cursor.Position);
                var tile = _map.GetTile(point);

                _contextMenu = new TileForm(tile);
                _contextMenu.StartPosition = FormStartPosition.Manual;
                _contextMenu.Location = canvas.PointToScreen(point);
                _contextMenu.Show(this);
                _contextMenu.FormClosing += (o, s) =>
                {
                    if (!s.Cancel)
                    {
                        canvas.Invalidate();
                    }
                };
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp",
                Title = "Select a Background File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                backgroundTxt.Text = dialog.FileName;
                _map.Background = !string.IsNullOrWhiteSpace(backgroundTxt.Text)
                    ? Image.FromFile(backgroundTxt.Text)
                    : null;
                canvas.Invalidate();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _map.ShowGrid = gridChk.Checked;
            canvas.Invalidate();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Map File|*.map",
                Title = "Save File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var json = JsonConvert.SerializeObject(_map.Save());
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.map",
                Title = "Select a File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var json = File.ReadAllText(dialog.FileName);
                var settings = JsonConvert.DeserializeObject<MapSettings>(json);

                _map = new Map(new WinFormGraphics(canvas), settings);
                _map.Init();
                canvas.Invalidate();
            }
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            var tile = ((Tile) ((Button) sender).Tag);
            button2.DoDragDrop(tile, DragDropEffects.Copy);
        }

        private void canvas_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tile)))
            {
                var dropPoint = new Point(e.X, e.Y);
                var point = canvas.PointToClient(dropPoint);
                var tile = (Tile)e.Data.GetData(typeof(Tile));
                var target = _map.GetTile(point);
                target.Image = tile.Image;
                thumbnail.Visible = false;
                canvas.Invalidate();
            }
        }

        private void canvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tile)))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy;
                return;
            }
            e.Effect = DragDropEffects.None;
        }

        private void canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tile)))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy;

                var tile = (Tile)e.Data.GetData(typeof(Tile));

                thumbnail.Width = tile.Image.Width;
                thumbnail.Height = tile.Image.Height;
                thumbnail.Location = canvas.PointToClient(Cursor.Position);
                thumbnail.Image = tile.Image;
                thumbnail.BringToFront();
                thumbnail.Visible = true;
                return;
            }
            e.Effect = DragDropEffects.None;
        }

        // Set transperancy
        //var bitmap = new Bitmap(tile.Image);
        //    for (var w = 0; w<bitmap.Width; w++)
        //{
        //    for (var h = 0; h<bitmap.Height; h++)
        //    {
        //        var c = bitmap.GetPixel(w, h);
        //        if (c != Color.Transparent)
        //        {
        //            var newC = Color.FromArgb(200, c.R, c.G, c.B);
        //            bitmap.SetPixel(w, h, newC);
        //        }
        //    }
        //}
        //thumbnail.Image = bitmap;
    }
}
