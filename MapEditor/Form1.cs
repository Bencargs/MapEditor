using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
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
            _map = new Map(graphics, canvas.Width / 20, canvas.Height / 20);
            // todo: replace with an OnChecked handler
            _map.ShowGrid(gridChk.Checked);
            _map.Init();

            _unitController = new UnitController(graphics);
            //_unitController.CreateUnit(8, 8);

            LoadTerrains();

            var timer = new Timer();
            timer.Tick += Update;
            timer.Interval = 100; // in miliseconds
            timer.Start();
        }

        private void LoadTerrains()
        {
            var terrainImagePath = @"C:\Source\MapEditor\MapEditor\Map\Terrains";
            var imageFiles = Directory.GetFiles(terrainImagePath, "*.png");
            for (int i = 0; i < imageFiles.Length; i++)
            {
                var image = new Bitmap(imageFiles[i]);
                image.MakeTransparent(Color.Fuchsia);
                var tag = new Terrain(TerrainType.Land, image, 20, 20);
                var button = new Button
                {
                    BackgroundImage = image,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Size = new Size(70, 70),
                    Location = new System.Drawing.Point(0, i * 70),
                    Tag = tag
                };
                button.MouseDown += TileTab_MouseDown;
                tileTab.Controls.Add(button);
            }
        }

        private void Update(object sender, EventArgs e)
        {
            _map.Update();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            _map.Render();
        }

        private void Canvas_Click(object sender, EventArgs e)
        {
            _contextMenu?.Close();
            if ((e as MouseEventArgs)?.Button == MouseButtons.Right)
            {
                var point = ((MouseEventArgs) e).Location;// canvas.PointToClient(Cursor.Position);
                var tile = _map.GetTile(point);
                var terrain = _map.GetTerrain(tile.TerrainIndex);

                _contextMenu = new TileForm(terrain);
                _contextMenu.StartPosition = FormStartPosition.Manual;
                _contextMenu.Location = canvas.PointToScreen(point);
                _contextMenu.FormClosing += (o, s) =>
                {
                    if (!s.Cancel)
                    {
                        canvas.Invalidate();
                    }
                };
                _contextMenu.Show(this);
                
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = @"Image Files|*.bmp",
                Title = @"Select a Background File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //todo: re-implement
                // get image, have map set each tile individually
                // what do we set the TerrainTypes as - None for all?

                //backgroundTxt.Text = dialog.FileName;
                //_map.Background = !string.IsNullOrWhiteSpace(backgroundTxt.Text)
                //    ? Image.FromFile(backgroundTxt.Text)
                //    : null;
                //canvas.Invalidate();
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            _map.ShowGrid(gridChk.Checked);
            canvas.Invalidate();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = @"Map File|*.map",
                Title = @"Save File"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var path = Path.GetFullPath(dialog.FileName);

                var tempPath = Path.Combine(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory(), "TempMapSave");
                Directory.CreateDirectory(tempPath);
                var mapSettings = _map.Save();
                foreach (var t in mapSettings.Terrains.Values)
                {
                    if (t.Image == null)
                        continue;

                    var imagePath = Path.Combine(tempPath, $"{t.Key}.png");
                    t.Image.Save(imagePath, ImageFormat.Png);
                }
                var json = JsonConvert.SerializeObject(mapSettings);
                var mapFilePath = Path.Combine(tempPath, "map.json");
                File.WriteAllText(mapFilePath, json);

                ZipFile.CreateFromDirectory(tempPath, path);

                if (File.Exists(path))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = @"Image Files|*.map",
                Title = @"Select a File"
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

        private void TileTab_MouseDown(object sender, MouseEventArgs e)
        {
            var button = (Button) sender;
            var tile = (Terrain) button.Tag;
            button.DoDragDrop(tile, DragDropEffects.Copy);
        }

        private void Canvas_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Terrain)))
            {
                var dropPoint = new Point(e.X, e.Y);
                var point = canvas.PointToClient(dropPoint);
                var tile = (Terrain)e.Data.GetData(typeof(Terrain));
                _map.SetTile(point, tile);
                thumbnail.Visible = false;
                canvas.Invalidate();
            }
        }

        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Terrain)))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy;
                return;
            }
            e.Effect = DragDropEffects.None;
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Terrain)))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy;

                var terrain = (Terrain)e.Data.GetData(typeof(Terrain));

                thumbnail.Width = terrain.Image.Width;
                thumbnail.Height = terrain.Image.Height;
                thumbnail.Location = canvas.PointToClient(Cursor.Position);
                thumbnail.Image = terrain.Image;
                thumbnail.BringToFront();
                thumbnail.Visible = true;
                return;
            }
            e.Effect = DragDropEffects.None;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var tab = sidePanel.SelectedTab;
            foreach (Button t in tab.Controls)
            {
                t.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                var previous = (Terrain) t.Tag;
                var image = new Bitmap(t.BackgroundImage);
                var terrain = new Terrain(previous.TerrainType, image, previous.Width, previous.Height);
                t.Tag = terrain;
                t.Refresh();
            }
        }
    }
}
