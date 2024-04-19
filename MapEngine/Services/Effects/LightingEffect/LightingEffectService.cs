using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common;
using MapEngine.Factories;
using MapEngine.Services.Map;
using static MapEngine.Services.Effects.LightingEffect.LightingEffects;

namespace MapEngine.Services.Effects.LightingEffect
{
    public class LightingEffectService
    {
        private readonly GameTime _gameTime;
        private readonly MapService _mapService;

        public LightingEffectService(
            GameTime gameTime, 
            MapService mapService)
        {
            _gameTime = gameTime;
            _mapService = mapService;
        }

        public byte[] ApplyAmbientLight(Rectangle viewport, byte[] fieldOfView)
        {
            var currentTime = _gameTime.TimeOfDay;
            foreach (var light in _mapService.LightingEffects.Ambient)
            {
                var onTime = TimeSpan.FromHours(light.On);
                var offTime = TimeSpan.FromHours(light.Off);
                if (currentTime <= onTime && currentTime >= offTime)
                    continue;

                if (!TextureFactory.TryGetTexture(light.TextureId, out var texture))
                    continue;

                var center = new Vector2(texture.Width / 2f, texture.Height / 2f);
                if (!light.LineOfSight)
                {
                    DrawStaticTexture(viewport, fieldOfView, light, center, texture);
                    continue;
                }

                DrawLineOfSightTexture(viewport, fieldOfView, texture, light, center);
            }

            return fieldOfView;
        }

        private void DrawLineOfSightTexture(
            Rectangle viewport, 
            byte[] fieldOfView, 
            Texture texture, 
            AmbientLight light,
            Vector2 center)
        {
            var radius = (int)Math.Min(texture.Width / 2f, texture.Height / 2f);
            var elevation = _mapService.GetElevation(light.Location);
            var raycast = GenerateVisibilityRaycast(elevation, light.Location, center, radius);

            var mask = FillPolygon(raycast, texture.Width, texture.Height);

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    var rX = (int)((light.Location.X - radius) + x);
                    var rY = (int)((light.Location.Y - radius) + y);
                    if (rX >= _mapService.Width - 1 ||
                        rY >= _mapService.Height - 1 ||
                        rX <= 0 ||
                        rY <= 0)
                        continue;

                    if (!mask[x, y])
                        continue;

                    var colour = texture.Image[x, y];
                    var index = (rY * viewport.Width + rX) * 4;

                    var alpha = colour.Alpha / 255f;
                    fieldOfView[index + 0] = (byte)((fieldOfView[index + 0] * (1f - alpha)) + (colour.Red * alpha));
                    fieldOfView[index + 1] = (byte)((fieldOfView[index + 1] * (1f - alpha)) + (colour.Blue * alpha));
                    fieldOfView[index + 2] = (byte)((fieldOfView[index + 2] * (1f - alpha)) + (colour.Green * alpha));
                }
            }
        }

        private static void DrawStaticTexture(
            Rectangle viewport, 
            byte[] fieldOfView, 
            AmbientLight light, 
            Vector2 center,
            Texture texture)
        {
            var startX = (int)(light.Location.X - center.X);
            var startY = (int)(light.Location.Y - center.Y);

            for (var y = 0; y < texture.Width; y++)
            {
                for (var x = 0; x < texture.Height; x++)
                {
                    // Calculate the texture's current pixel position
                    var texturePos = (y * texture.Width + x) * 4; // 4 bytes per pixel

                    if ((texturePos + 3) > texture.Image.Buffer.Length)
                        continue;

                    // Skip processing if the pixel is fully transparent
                    if (texture.Image.Buffer[texturePos + 3] == 0)
                        continue;

                    // Calculate the corresponding position on the canvas
                    var canvasPosX = startX + x;
                    var canvasPosY = startY + y;

                    // Check if the current position is within the bounds of the canvas
                    if (canvasPosX < 0 || canvasPosX >= viewport.Width ||
                        canvasPosY < 0 || canvasPosY >= viewport.Height)
                        continue;

                    var canvasPos = (canvasPosY * viewport.Width + canvasPosX) * 4;

                    // Alpha blending
                    var alpha = texture.Image.Buffer[texturePos + 3] / 255f;
                    fieldOfView[canvasPos + 0] = (byte)(alpha * texture.Image.Buffer[texturePos + 0] +
                                                        (1f - alpha) * fieldOfView[canvasPos + 0]);
                    fieldOfView[canvasPos + 1] = (byte)(alpha * texture.Image.Buffer[texturePos + 1] +
                                                        (1f - alpha) * fieldOfView[canvasPos + 1]);
                    fieldOfView[canvasPos + 2] = (byte)(alpha * texture.Image.Buffer[texturePos + 2] +
                                                        (1f - alpha) * fieldOfView[canvasPos + 2]);
                }
            }
        }

        // Via poly fill algorithm
        private bool[,] FillPolygon(List<Vector2> points, int width, int height)
        {
            // Determine the bounds of the polygon
            int minX = (int)points.Min(p => p.X);
            int maxX = (int)points.Max(p => p.X);
            int minY = (int)points.Min(p => p.Y);
            int maxY = (int)points.Max(p => p.Y);

            //var mask = new bool[maxX - minX  + 1, maxY - minY + 1];
            var mask = new bool[width, height];

            // Iterate over each scan line
            for (int y = minY; y <= maxY; y++)
            {
                var intersections = new List<int>(); // List to store nodes (x-intersections)

                // Find intersections of the scanline with the polygon edges
                for (int i = 0; i < points.Count; i++)
                {
                    int j = (i + 1) % points.Count;
                    if ((points[i].Y < y && points[j].Y >= y) || (points[j].Y < y && points[i].Y >= y))
                    {
                        int x = (int)(points[i].X + (double)(y - points[i].Y) / (points[j].Y - points[i].Y) * (points[j].X - points[i].X));
                        intersections.Add(x);
                    }
                }

                // Sort the nodes, fill the pixels between node pairs.
                intersections.Sort();

                // Fill the scanline between pairs of intersections
                for (int i = 0; i < intersections.Count - 1; i += 2)
                {
                    for (int x = intersections[i]; x <= intersections[i + 1] - 1; x++)
                    {
                        mask[y - minY, x] = true;
                    }
                }
            }

            return mask;
        }

        private List<Vector2> GenerateVisibilityRaycast(
            int entityHeight,
            Vector2 entityLocation,
            Vector2 center,
            int radius)
        {
            var results = new List<Vector2>();
            const int arcDegrees = 360; // todo: this should be start unit property
            foreach (var c in GetCircle(center, arcDegrees, radius))
            {
                // todo: reassess the fudge factor here
                var maxRayHeight = entityHeight;
                foreach (var r in GetRay(center, c))
                {
                    // If we're at an edge of the map, stop ray casting
                    var mapLocation = (r - center) + entityLocation;
                    if (mapLocation.X < 0 || mapLocation.X >= _mapService.Width||
                        mapLocation.Y < 0 || mapLocation.Y >= _mapService.Height)
                    {
                        results.Add(r);
                        break;
                    }

                    // Test for intersections above maximum view elevation
                    var mapHeight = _mapService.GetElevation(mapLocation);
                    if (mapHeight > maxRayHeight++) // assuming horizontal vision is start 45 degree arc
                    {
                        results.Add(r);
                        break;
                    }

                    // If ray reaches maximum LOS, stop ray casting
                    if (r == c)
                    {
                        results.Add(r);
                        break;
                    }
                }
            }
            return results;
        }

        private static IEnumerable<Vector2> GetRay(Vector2 start, Vector2 end)
        {
            var startX = (int)start.X;
            var startY = (int)start.Y;
            var endX = (int)end.X;
            var endY = (int)end.Y;

            int dx = Math.Abs(endX - startX), sx = startX < endX ? 1 : -1;
            int dy = Math.Abs(endY - startY), sy = startY < endY ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 4, e2;

            var x0 = startX;
            var y0 = startY;
            while (true)
            {
                if (x0 == endX && y0 == endY) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
                yield return new Vector2(x0, y0);
            }
        }

        private static IEnumerable<Vector2> GetCircle(Vector2 location, int arcDegrees, float radius)
        {
            // Calculate the start and end angles of the arc
            var startAngle = 0; // Assuming 0 degrees is at 3 o'clock position

            // Calculate the number of points to generate based on the degree increment
            var degreeIncrement = 1f; // You can adjust this value for start smoother or more granular arc
            var numPoints = (int)Math.Ceiling(arcDegrees / degreeIncrement);

            for (int i = 0; i <= numPoints; i++)
            {
                // Calculate the current angle for the point
                var angle = startAngle + (i * degreeIncrement);

                // Convert angle to radians
                var radians = angle * (Math.PI / 180.0);

                // Calculate the X and Y coordinates for the current point on the arc
                var x = location.X + (float)Math.Round(radius * Math.Cos(radians));
                var y = location.Y + (float)Math.Round(radius * Math.Sin(radians));

                yield return new Vector2(x, y);
            }
        }

        public byte[] GenerateBitmap(Rectangle viewport)
        {
            var fieldOfView = new byte[viewport.Width * viewport.Height * 4];

            ApplyDiffuseLight(viewport, fieldOfView);
            ApplyAmbientLight(viewport, fieldOfView);

            return fieldOfView;
        }

        /// <summary>
        /// Scene wide lighting - eg sunlight
        /// </summary>
        /// <param name="_"></param>
        /// <param name="fieldOfView"></param>
        /// <returns></returns>
        public byte[] ApplyDiffuseLight(Rectangle _, byte[] fieldOfView)
        {
            // Apply time of day shading
            var weight = GetColorWeightForTime(_gameTime.TimeOfDay);

            int totalLength = fieldOfView.Length;
            for (int i = 0; i < totalLength; i += 4)
            {
                fieldOfView[i] = weight.Red;
                fieldOfView[i + 1] = weight.Blue;
                fieldOfView[i + 2] = weight.Green;
                fieldOfView[i + 3] = weight.Alpha;
            }

            return fieldOfView;
        }

        public Colour GetColorWeightForTime(TimeSpan currentTime)
        {
            var diffuseLights = _mapService.LightingEffects.Diffuse;

            // Find the current light and its index
            DiffuseLight currentLight = null;
            int index = -1;
            for (int i = 0; i < diffuseLights.Length; i++)
            {
                var light = diffuseLights[i];
                TimeSpan onTime = TimeSpan.FromHours(light.On);
                TimeSpan offTime = TimeSpan.FromHours(light.Off);

                if ((onTime <= offTime && currentTime >= onTime && currentTime < offTime) ||
                    (onTime > offTime && (currentTime >= onTime || currentTime < offTime)))
                {
                    currentLight = light;
                    index = i;
                    break;
                }
            }

            if (currentLight is null)
                return new Colour(0, 0, 0, 0);

            if (currentLight.TransitionType == TransitionType.Fade)
            {
                TimeSpan onTime = TimeSpan.FromHours(currentLight.On);
                TimeSpan offTime = TimeSpan.FromHours(currentLight.Off);
                var midpoint = TimeSpan.FromHours((currentLight.On + currentLight.Off) / 2.0);

                if (currentTime < midpoint)
                {
                    float fraction = (float)(currentTime - onTime).TotalHours / (float)(midpoint - onTime).TotalHours;
                    var previousLight = index != 0 ? diffuseLights[index - 1] : diffuseLights.Last();
                    return previousLight.Colour.Interpolate(currentLight.Colour, fraction);
                }
                else
                {
                    float fraction = (float)(currentTime - midpoint).TotalHours / (float)(offTime - midpoint).TotalHours;
                    var nextLight = (index < diffuseLights.Length - 1) ? diffuseLights[index + 1] : diffuseLights.First();
                    return currentLight.Colour.Interpolate(nextLight.Colour, fraction);
                }
            }

            return currentLight.Colour;
        }
    }
}
