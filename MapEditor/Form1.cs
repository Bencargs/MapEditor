﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Common;
using MapEditor.Commands;
using MapEditor.Components;
using MapEditor.Engine;
using MapEditor.Entities;
using MapEditor.File;
using MapEditor.Handlers;
using MapEditor.Repository;
using Newtonsoft.Json;
using Rectangle = System.Drawing.Rectangle;

namespace MapEditor
{
    public partial class Form1 : Form
    {
        private readonly MessageHub _messageHub;
        private readonly ISession _session;
        private readonly UnitHandler _unitHandler;
        private readonly Editor.EditorInput _input;
        private TileForm _contextMenu;
        private Editor.MapEditor _mapHandler;
        private Scene _scene;
        private CameraHandler _cameraHandler;
        private MouseState _mouseState;

        public Form1()
        {
            InitializeComponent();

            _messageHub = new MessageHub();
            _session = new Session(_messageHub);
            _session.Init();

            var graphics = new WinFormGraphics(canvas);
            _scene = new Scene(_session, graphics);

            _mapHandler = new Editor.MapEditor(_messageHub, _session, canvas.Width / 20, canvas.Height / 20);
            // todo: replace with an OnChecked handler
            _mapHandler.ShowGrid(gridChk.Checked);
            _mapHandler.Init();

            _unitHandler = new UnitHandler(_messageHub, _session);
            _unitHandler.Init();

            // todo: is cameraHandler the responsibility of Map class?
            _cameraHandler = new CameraHandler(_messageHub, new Point(0, 0), canvas.Width, canvas.Height);
            _cameraHandler.Init();

            _input = new Editor.EditorInput(_messageHub, _cameraHandler);
            _mouseState = new MouseState();
            canvas.MouseMove += (sender, eventArgs) =>
            {
                _mouseState.GetState(eventArgs.Location, eventArgs.Button);
                _input.OnMouseEvent(_mouseState);
            };
            KeyPreview = true;
            KeyPress += (sender, eventArgs) =>
            {
                _input.OnKeyboardEvent(eventArgs);
            };

            LoadTerrains();
            LoadUnits();

            var timer = new Timer();
            timer.Tick += Update;
            timer.Interval = 100; // in miliseconds
            timer.Start();
        }

        private void LoadTerrains()
        {
            var i = 0;
            const string terrainImagePath = @"C:\Source\MapEditor\MapEditor\Map\Terrains";
            foreach (var image in LoadImages(terrainImagePath))
            {
                var terrain = new Terrain(TerrainType.Land, image, 20, 20);
                var button = new Button
                {
                    BackgroundImage = image,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Size = new Size(70, 70),
                    Location = new Point(0, i++ * 70),
                    Tag = terrain
                };
                button.MouseDown += TileTab_MouseDown;
                tileTab.Controls.Add(button);
            }
        }

        private void LoadUnits()
        {
            const string unitPath = @"C:\Source\MapEditor\MapEditor\Units";
            var unitFiles = Directory.GetFiles(unitPath, "*.unit");
            var templates = unitFiles.Select(_unitHandler.LoadTemplate);

            var i = 0;
            foreach (var t in templates)
            {
                var imageComponent = t.GetComponent<ImageComponent>();
                if (imageComponent == null)
                    return;

                var button = new Button
                {
                    BackgroundImage = imageComponent.Image,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Size = new Size(70, 70),
                    Location = new Point(0, i++ * 70),
                    Tag = t
                };
                button.MouseDown += TileTab_MouseDown;
                unitTab.Controls.Add(button);
            }
        }

        private static IEnumerable<Bitmap> LoadImages(string path)
        {
            var bitmaps = new List<Bitmap>();
            var imageFiles = Directory.GetFiles(path, "*.png");
            foreach (var t in imageFiles)
            {
                var image = new Bitmap(t);
                image.MakeTransparent(Color.Fuchsia);
                bitmaps.Add(image);
            }
            return bitmaps;
        }

        private void Update(object sender, EventArgs e)
        {
            _scene.Render();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            //_map.Render();
            //_unitHandler.Render();
        }

        private void Canvas_Click(object sender, EventArgs e)
        {
            _contextMenu?.Close();
            if ((e as MouseEventArgs)?.Button == MouseButtons.Right)
            {
                var point = ((MouseEventArgs) e).Location;
                var tile = _session.GetTile(point);
                var terrain = _session.GetTerrain(tile.TerrainIndex);

                _contextMenu = new TileForm(terrain);
                _contextMenu.StartPosition = FormStartPosition.Manual;
                _contextMenu.Location = canvas.PointToScreen(point);
                _contextMenu.FormClosing += (o, s) =>
                {
                    if (!_contextMenu.Cancel)
                    {
                        var newTerrain = ((TileForm) o).Terrain;
                        _mapHandler.SetTile(point, newTerrain);
                    }
                };
                _contextMenu.Show(this);
            }
        }

        private void ImportBackground_Click(object sender, EventArgs e)
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
            // todo: these should be editor concerns, not map concerns
            _mapHandler.ShowGrid(gridChk.Checked);
        }

        private void terrainChk_CheckedChanged(object sender, EventArgs e)
        {
            _mapHandler.ShowTerrain(terrainChk.Checked);
        }

        private MapFile Save()
        {
            var settings = _session.GetMapSettings();
            return new MapFile
            {
                Width = settings.Width,
                Height = settings.Height,
                ShowGrid = settings.ShowGrid,
                ShowTerrain = settings.ShowTerrain,
                Tiles = _session.GetTiles(),
                Terrains = _session.GetTerrains(),
                Units = _session.GetUnits(),
                Viewport = _session.GetViewport()

            };
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = @"Map File|*.map",
                Title = @"Save File",
                InitialDirectory = @"C:\Source\MapEditor\MapEditor\Map\Saves"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var path = Path.GetFullPath(dialog.FileName);

                var tempPath = Path.Combine(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory(), "TempMapSave");
                Directory.CreateDirectory(tempPath);
                var mapFile = Save();
                foreach (var t in mapFile.Terrains)
                {
                    if (t.Value.Image == null)
                        continue;
                    
                    var imagePath = Path.Combine(tempPath, $"{t.Key}.png");
                    t.Value.Image.Save(imagePath, ImageFormat.Png);
                }
                var json = JsonConvert.SerializeObject(mapFile);
                var mapFilePath = Path.Combine(tempPath, "map.json");
                System.IO.File.WriteAllText(mapFilePath, json);

                ZipFile.CreateFromDirectory(tempPath, path);

                if (System.IO.File.Exists(path))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // todo: helper class to set relative paths
            var dialog = new OpenFileDialog
            {
                Filter = @"Image Files|*.map",
                Title = @"Select a File",
                InitialDirectory = @"C:\Source\MapEditor\MapEditor\Map\Saves"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                MapFile data = null;

                var archive = ZipFile.OpenRead(dialog.FileName);
                var mapdata = archive.GetEntry("map.json");
                if (mapdata != null)
                {
                    using (var stream = mapdata.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        data = JsonConvert.DeserializeObject<MapFile>(json);
                    }

                    data.Terrains = data.Terrains.Select(x =>
                    {
                        // todo: customer serializer/deserializer
                        Bitmap tileImage = null;
                        var imageData = archive.GetEntry($"{x.Key}.png");
                        if (imageData != null)
                        {
                            using (var stream = imageData.Open())
                            using (var image = Image.FromStream(stream))
                            {
                                tileImage = new Bitmap(image);
                            }
                        }
                        return new Terrain(x.Value.TerrainType, tileImage, x.Value.Width, x.Value.Height);
                    }).ToDictionary(k => k.Key);
                }

                if (data != null)
                {
                    _mapHandler = new Editor.MapEditor(_messageHub, _session, data.Width, data.Height);
                }
            }
        }

        private void TileTab_MouseDown(object sender, MouseEventArgs e)
        {
            var button = (Button) sender;
            button.DoDragDrop(button.Tag, DragDropEffects.Copy);
        }

        private void Canvas_DragDrop(object sender, DragEventArgs e)
        {
            var dropPoint = new Point(e.X, e.Y);
            var point = canvas.PointToClient(dropPoint);

            if (e.Data.GetDataPresent(typeof(Terrain)))
            {
                var terrain = (Terrain)e.Data.GetData(typeof(Terrain));
                var area = new Rectangle(point.X, point.Y, terrain.Width, terrain.Height);
                _messageHub.Post(new PlaceTileCommand
                {
                    Point = point,
                    Terrain = terrain,
                    PreviousTile = _session.GetTiles(area)
                });
                thumbnail.Visible = false;
            }
            else if (e.Data.GetDataPresent(typeof(Entity)))
            {
                var unit = (Entity)e.Data.GetData(typeof(Entity));
                _messageHub.Post(new AddUnitCommand
                {
                    Point = point,
                    Unit = unit
                });
                thumbnail.Visible = false;
                canvas.Invalidate();
            }
        }

        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(typeof(Terrain)) ||
                e.Data.GetDataPresent(typeof(Entity)))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy;
            }
            //{
            //    var unit = (Entity)e.Data.GetData(typeof(Entity));
            //    var unitCollider = unit.GetComponent<CollisionComponent>().Collider;

            //    var point = new Point(e.X, e.Y);
            //    var tile = _map.GetTile(point);
            //    var colliders = tile.GetUnits().Select(x => x.GetComponent<CollisionComponent>().Collider);
            //    if (colliders.Any(c => c.IsCollided(unitCollider)))
            //    {
            //        return;
            //    }
            //    e.Effect = DragDropEffects.Move | DragDropEffects.Copy;
            //}
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
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
            if (e.Data.GetDataPresent(typeof(Entity)))
            {
                // Ensure we arn't overlapping any existing units
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy;

                var unit = (Entity)e.Data.GetData(typeof(Entity));

                var imageComponent = unit.GetComponent<ImageComponent>();
                thumbnail.Width = imageComponent.Image.Width;
                thumbnail.Height = imageComponent.Image.Height;
                thumbnail.Location = canvas.PointToClient(Cursor.Position);
                thumbnail.Image = imageComponent.Image;
                thumbnail.BringToFront();
                thumbnail.Visible = true;
            }
        }

        private void RotateClick(object sender, EventArgs e)
        {
            var tab = sidePanel.SelectedTab;
            foreach (Button t in tab.Controls)
            {
                t.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

                if (!(t.Tag is Terrain))
                    return;

                var previous = (Terrain) t.Tag;
                var image = new Bitmap(t.BackgroundImage);
                var terrain = new Terrain(previous.TerrainType, image, previous.Width, previous.Height);
                t.Tag = terrain;
                t.Refresh();
            }
        }
    }
}
