﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MapEditor.Common;
using MapEditor.Components;
using MapEditor.Entities;
using Newtonsoft.Json;

namespace MapEditor
{
    public enum TerrainType
    {
        Empty = 0,
        Water,
        Land,
        ImpassableLand,
    }

    public struct Terrain : IDisposable
    {
        public Guid Key { get; }

        public int Width { get; }
        public int Height { get; }
        [JsonIgnore]
        public Bitmap Image { get; }
        public TerrainType TerrainType { get; }

        public Terrain(TerrainType terrainType, Bitmap image, int width, int height)
        {
            Image = image;
            Width = width;
            Height = height;
            TerrainType = terrainType;
            Key = Guid.Empty;

            Key = new Guid(Image.GetImageHashcode());
        }
        
        public override int GetHashCode()
        {
            return Key.GetHashCode() ^ TerrainType.GetHashCode() ^ Width ^ Height;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = (Terrain) obj;
            return TerrainType == other.TerrainType && 
                   Width == other.Width &&
                   Height == other.Height && 
                   /*ImageCompare(other.Image)*/
                   Key == other.Key;
        }

        private bool ImageCompare(object obj)
        {
            var secondImage = obj as Bitmap;
            if (secondImage == null)
            {
                return Image == null;
            }

            if (Image?.Width != secondImage.Width || Image.Height != secondImage.Height)
                return false;

            for (var i = 0; i < Image.Width; i++)
            {
                for (var j = 0; j < Image.Height; j++)
                {
                    var firstPixel = Image.GetPixel(i, j).ToString();
                    var secondPixel = secondImage.GetPixel(i, j).ToString();
                    if (firstPixel != secondPixel)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }

    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public bool IsDirty { get; set; }
        public Guid TerrainIndex { get; set; }
        public List<ICollider> Colliders { get; set; }
        
        public Tile(int x, int y, Guid terrainIndex)
        {
            X = x;
            Y = y;
            TerrainIndex = terrainIndex;
            IsDirty = true;
        }

        //public void Render(IGraphics graphics)
        //{
        //    if (Terrain.Image != null)
        //    {
        //        var area = new Rectangle(X, Y, Terrain.Width, Terrain.Height);
        //        graphics.DrawImage(Terrain.Image, area);
        //    }
        //}
    }
}
