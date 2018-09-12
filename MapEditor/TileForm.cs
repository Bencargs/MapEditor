﻿using System;
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
    public partial class TileForm : Form
    {
        public Terrain Terrain { get; set; }

        public TileForm(Terrain terrain)
        {
            InitializeComponent();

            Terrain = terrain;
            var comboItems = ((TerrainType[]) Enum.GetValues(typeof(TerrainType))).Select(x => new FormattedTerrainItem(x));
            terrainCmb.Items.AddRange(comboItems.ToArray());
            terrainCmb.SelectedIndex = (int)Terrain.TerrainType;
            terrainCmb.ValueMember = "Value";
            terrainCmb.DisplayMember = "Description";
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Accept_Click(object sender, EventArgs e)
        {
            if (terrainCmb?.SelectedItem != null)
            {
                //todo: _map.SetTerrain
                var previous = Terrain;
                var type = ((FormattedTerrainItem) terrainCmb.SelectedItem).Value;
                Terrain = new Terrain(type, previous.Image, previous.Width, previous.Height);
            }
            Close();
        }
    }

    internal sealed class FormattedTerrainItem
    {
        public string Description => Enum.GetName(typeof(TerrainType), Value);
        public TerrainType Value { get; }

        public FormattedTerrainItem(TerrainType terrain)
        {
            Value = terrain;
        }
    }
}
