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

        /// <summary>
        /// Static lighting effects - eg streetlights
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="fieldOfView"></param>
        /// <returns></returns>
        public byte[] ApplyAmbientLight(Rectangle viewport, byte[] fieldOfView)
        {
            var currentTime = _gameTime.TimeOfDay;
            var canvasWidth = viewport.Width;
            var canvasHeight = viewport.Height;

            foreach (var light in _mapService.LightingEffects.Ambient)
            {
                var onTime = TimeSpan.FromHours(light.On);
                var offTime = TimeSpan.FromHours(light.Off);
                if (currentTime <= onTime && currentTime >= offTime)
                    continue;

                if (!TextureFactory.TryGetTexture(light.TextureId, out var texture)) 
                    continue;

                var center = new Vector2(texture.Width / 2f, texture.Height / 2f);
                var startX = (int)(light.Location.X - center.X);
                var startY = (int)(light.Location.Y - center.Y);

                // todo: dynamic shadows
                //var radius = Math.Max(texture.Width, texture.Height) / 2;
                //var raycast = GenerateVisibilityRaycast(light.Location, center, radius);
                //foreach (var r in raycast)
                //{
                //    var rX = (int) r.X;
                //    var rY = (int)r.Y;

                //    var texturePos = (rY * texture.Width + rX) * 4;
                //    if ((texturePos + 3) > texture.Image.Buffer.Length)
                //        continue;
                //    // Skip processing if the pixel is fully transparent
                //    if (texture.Image.Buffer[texturePos + 3] == 0)
                //        continue;

                //    var canvasPosX = startX + rX;
                //    var canvasPosY = startY + rY;
                //    var canvasPos = (canvasPosY * canvasWidth + canvasPosX) * 4;
                //    // Check if the current position is within the bounds of the canvas
                //    if (canvasPosX < 0 || canvasPosX >= canvasWidth ||
                //        canvasPosY < 0 || canvasPosY >= canvasHeight)
                //        continue;

                //    var alpha = texture.Image.Buffer[texturePos + 3] / 255f;
                //    fieldOfView[canvasPos + 0] = (byte)(alpha * texture.Image.Buffer[texturePos + 0] + (1f - alpha) * fieldOfView[canvasPos + 0]);
                //    fieldOfView[canvasPos + 1] = (byte)(alpha * texture.Image.Buffer[texturePos + 1] + (1f - alpha) * fieldOfView[canvasPos + 1]);
                //    fieldOfView[canvasPos + 2] = (byte)(alpha * texture.Image.Buffer[texturePos + 2] + (1f - alpha) * fieldOfView[canvasPos + 2]);
                //    fieldOfView[canvasPos + 3] = (byte)(alpha * texture.Image.Buffer[texturePos + 3] + (1f - alpha) * fieldOfView[canvasPos + 3]);
                //}

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
                        if (canvasPosX < 0 || canvasPosX >= canvasWidth ||
                            canvasPosY < 0 || canvasPosY >= canvasHeight)
                            continue;

                        var canvasPos = (canvasPosY * canvasWidth + canvasPosX) * 4;

                        // Alpha blending
                        var alpha = texture.Image.Buffer[texturePos + 3] / 255f;
                        fieldOfView[canvasPos + 0] = (byte)(alpha * texture.Image.Buffer[texturePos + 0] + (1f - alpha) * fieldOfView[canvasPos + 0]);
                        fieldOfView[canvasPos + 1] = (byte)(alpha * texture.Image.Buffer[texturePos + 1] + (1f - alpha) * fieldOfView[canvasPos + 1]);
                        fieldOfView[canvasPos + 2] = (byte)(alpha * texture.Image.Buffer[texturePos + 2] + (1f - alpha) * fieldOfView[canvasPos + 2]);
                    }
                }
            }

            return fieldOfView;
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

        private IEnumerable<Vector2> GenerateVisibilityRaycast(Vector2 entityLocation, Vector2 center, int radius)
        {
            const double angleIncrement = 0.5; // Angle increment in degrees, adjust for precision

            var entityHeight = _mapService.GetElevation(center);
            for (double angle = 0; angle < 360; angle += angleIncrement)
            {
                var radians = Math.PI * angle / 180.0;
                var endX = center.X + radius * Math.Cos(radians);
                var endY = center.Y + radius * Math.Sin(radians);
                var end = new Vector2((float)endX, (float)endY);

                foreach (var r in GetRay(center, end))
                {
                    yield return r;

                    var mapLocation = (r - center) + entityLocation;
                    if (mapLocation.X < 0 || mapLocation.X >= _mapService.Width - 1 ||
                        mapLocation.Y < 0 || mapLocation.Y >= _mapService.Height - 1)
                    {
                        break;
                    }

                    var mapHeight = _mapService.GetElevation(new Vector2((int)r.X, (int)r.Y));
                    if (mapHeight > entityHeight)
                    {
                        break;
                    }
                }
            }
        }

        private static IEnumerable<Vector2> GetRay(Vector2 start, Vector2 end)
        {
            var startX = (int)start.X;
            var startY = (int)start.Y;
            var endX = (int)end.X;
            var endY = (int)end.Y;

            int dx = Math.Abs(endX - startX), sx = startX < endX ? 1 : -1;
            int dy = Math.Abs(endY - startY), sy = startY < endY ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;

            int x = startX;
            int y = startY;
            while (true)
            {
                if (x == endX && y == endY) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x += sx; }
                if (e2 < dy) { err += dx; y += sy; }

                yield return new Vector2(x, y);
            }
        }
    }
}
