using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Engine;

namespace MapEditor.Commands
{
    public class PlaceTileCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.PlaceTile;

        public Point Point { get; set; }
        public Terrain Terrain { get; set; }
        public List<Tile> PreviousTerrain { get; set; }    //todo: lazy initialize?
    }
}
