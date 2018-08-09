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
    public partial class TileForm : Form
    {
        private readonly Tile _tile;

        public TileForm(Tile tile)
        {
            InitializeComponent();

            _tile = tile;
            var comboItems = ((Terrain[]) Enum.GetValues(typeof(Terrain))).Select(x => new FormattedTerrainItem(x));
            terrainCmb.Items.AddRange(comboItems.ToArray());
            terrainCmb.SelectedIndex = (int) _tile.Terrain;
            terrainCmb.ValueMember = "Value";
            terrainCmb.DisplayMember = "Description";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void accept_Click(object sender, EventArgs e)
        {
            if (terrainCmb?.SelectedItem != null)
            {
                _tile.Terrain = ((FormattedTerrainItem) terrainCmb.SelectedItem).Value;
                _tile.IsDirty = true;
            }
            Close();
        }
    }

    sealed class FormattedTerrainItem
    {
        public string Description => Enum.GetName(typeof(Terrain), Value);
        public Terrain Value { get; }

        public FormattedTerrainItem(Terrain terrain)
        {
            Value = terrain;
        }
    }
}
