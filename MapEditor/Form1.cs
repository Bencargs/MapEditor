﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MapEditor
{
    public partial class Form1 : Form
    {
        private TileForm _contextMenu;
        private Map _map;

        public Form1()
        {
            InitializeComponent();
            _map = new Map(new WinFormGraphics(canvas), canvas.Width / 20, canvas.Height / 20)
                {
                    ShowGrid = gridChk.Checked
                };

            var timer = new Timer();
            timer.Tick += Update;
            timer.Interval = 500; // in miliseconds
            timer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            _map.Update();
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            //canvas.Invalidate();
            _map.Render();
        }

        private void canvas_Click(object sender, EventArgs e)
        {
            _contextMenu?.Close();
            if ((e as MouseEventArgs)?.Button == MouseButtons.Right)
            {
                var point = canvas.PointToClient(Cursor.Position);
                var tile = _map.GetCell(point);

                _contextMenu = new TileForm(tile);
                _contextMenu.SetDesktopLocation(point.X, point.Y);
                _contextMenu.Show();
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
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _map.ShowGrid = gridChk.Checked;
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
                var json = JsonConvert.SerializeObject(_map);
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
                //_map = JsonConvert.DeserializeObject<Map>(json);
            }
        }
    }
}
