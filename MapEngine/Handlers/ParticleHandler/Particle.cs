﻿using System.Numerics;

namespace MapEngine.Handlers.ParticleHandler
{
    public class Particle
    {
        public Vector2 Location { get; set; }
        public Vector2 Velocity { get; set; }
        public string TextureId { get; set; }
        public float Lifetime { get; set; }
        public float FacingAngle { get; set; }
        public float Fade { get; set; }
        public float Size { get; set; }
        public int HueIndex { get; set; }
        public string PaletteTextureId { get; set; }
        public int PalleteSpeed { get; set; }
    }
}
