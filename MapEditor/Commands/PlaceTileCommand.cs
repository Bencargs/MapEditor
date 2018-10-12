using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Engine;

namespace MapEditor.Commands
{
    public class PlaceTileCommand : IReversableCommand
    {
        public CommandType Id { get; } = CommandType.PlaceTile;

        public Point Point { get; set; }
        public Terrain Terrain { get; set; }
        public List<Tile> PreviousTile { get; set; }    //todo: lazy initialize?
    }
}
