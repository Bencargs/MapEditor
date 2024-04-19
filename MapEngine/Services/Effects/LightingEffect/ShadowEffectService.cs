using Common;
using MapEngine.Services.Map;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MapEngine.Services.Effects.LightingEffect
{
    public class ShadowEffectService
    {
        private readonly GameTime _gameTime;
        private readonly MapService _mapService;
        private List<Vector3> _edgePoints;

        public ShadowEffectService(
            GameTime gameTime, 
            MapService mapService)
        {
            _gameTime = gameTime;
            _mapService = mapService;
        }

        public void Initialise()
        {
            _edgePoints = FindEdges();
        }

        public byte[] GenerateBitmap(Rectangle viewport)
        {
            var fieldOfView = new byte[viewport.Width * viewport.Height * 4];

            var (sunDirection, shadowIntensity) = GetSunDirectionVector(_gameTime.TimeOfDay);

            // Find all the edges of regions on the heightmap - reduces processing vs checking every pixel
            var shadowsHashSet = new HashSet<Vector2>();
            foreach (var edge in _edgePoints)
            {
                // for each point on the heightmap edges, project a shadow using sun vector
                var shadowProjection = CastShadowFromPoint(edge, sunDirection);
                if (!shadowProjection.HasValue)
                    continue;

                // trace back all pixels along sun vector from project point to edge
                var shadowRay = DrawShadowRay(edge, shadowProjection.Value);
                foreach (var shadowPoint in shadowRay)
                {
                    // this prevents double rendering shadows on a pixel
                    if (!shadowsHashSet.Add(shadowPoint))
                        continue;

                    // only draw a shadow if it is projected below the terrains height
                    //var originalColor = texture.GetPixel(shadowPoint.X, shadowPoint.Y);
                    var shadowHeight = _mapService.GetElevation(new Vector2(shadowPoint.X, shadowPoint.Y));
                    if (shadowHeight > edge.Z)
                        continue;

                    var index = (int)(shadowPoint.Y * viewport.Width + shadowPoint.X) * 4;
                    fieldOfView[index + 3] = (byte)(fieldOfView[index + 3] + (148 * shadowIntensity));
                }
            }

            return fieldOfView;
        }

        private static (Vector2 Direction, float Intensity) GetSunDirectionVector(TimeSpan currentTime)
        {
            // todo: from config?
            // Define key times of the day.
            var morningStart = TimeSpan.FromHours(4f);  // 6 AM
            var midday = TimeSpan.FromHours(12);       // 12 PM
            var eveningEnd = TimeSpan.FromHours(20);   // 6 PM

            // Initialize the sun direction vector.
            var sunDirection = new Vector2(0, 0);
            var sunIntensity = 0f;
            var shadowLength = 100;

            if (currentTime >= morningStart && currentTime <= midday)
            {
                // Interpolate from (-1, 1) at 6 AM to (0, 0) at midday.
                var progress = (float)(currentTime - morningStart).TotalHours / (float)(midday - morningStart).TotalHours;
                sunDirection = new Vector2(-shadowLength, shadowLength - (shadowLength * progress));
                sunIntensity = (progress);
            }
            else if (currentTime > midday && currentTime <= eveningEnd)
            {
                // Interpolate from (0, 0) at midday to (1, -1) at 6 PM.
                var progress = (float)(currentTime - midday).TotalHours / (float)(eveningEnd - midday).TotalHours;
                sunDirection = new Vector2(-shadowLength, (shadowLength * -progress));
                sunIntensity = (1 - progress);
            }
            // From 6 PM to 6 AM, sunDirection remains at (0, 0), as initialized.

            return (sunDirection, sunIntensity);
        }

        //todo: third time, please refactor

        // Bresenham's line algorithm variables
        private static List<Vector2> DrawShadowRay(Vector3 start, Vector2 end)
        {
            int dx = (int)Math.Abs(end.X - start.X), sx = start.X < end.X ? 1 : -1;
            int dy = (int)-Math.Abs(end.Y - start.Y), sy = start.Y < end.Y ? 1 : -1;
            int err = dx + dy;

            var shadowRayPoints = new List<Vector2>();
            while (true)
            {
                if (start.X == end.X && start.Y == end.Y) break;

                var e2 = 2 * err;
                if (e2 >= dy)
                {
                    if (start.X == end.X) break;
                    err += dy; start.X += sx;
                }
                if (e2 <= dx)
                {
                    if (start.Y == end.Y) break;
                    err += dx; start.Y += sy;
                }
                shadowRayPoints.Add(new Vector2(start.X, start.Y));
            }
            return shadowRayPoints;
        }

        // todo: move to common library
        private List<Vector3> FindEdges()
        {
            var edgePoints = new List<Vector3>();

            var width = _mapService.Width;
            var height = _mapService.Height;

            var gx = new[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            var gy = new[,]
            {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 }
            };

            // Define threshold for what is considered an edge
            var edgeThreshold = 128;

            for (var i = 1; i < width - 1; i++)
            {
                for (var j = 1; j < height - 1; j++)
                {
                    float xGradient = 0;
                    float yGradient = 0;

                    for (var xi = -1; xi <= 1; xi++)
                    {
                        for (var yi = -1; yi <= 1; yi++)
                        {
                            var pixelIntensity = _mapService.GetElevation(new Vector2(i + xi, j + yi));

                            xGradient += pixelIntensity * gx[xi + 1, yi + 1];
                            yGradient += pixelIntensity * gy[xi + 1, yi + 1];
                        }
                    }

                    int gradientMagnitude = (int)Math.Sqrt(xGradient * xGradient + yGradient * yGradient);

                    // If the gradient magnitude exceeds the threshold, consider it an edge
                    if (gradientMagnitude > edgeThreshold)
                    {
                        var elevation = _mapService.GetElevation(new Vector2(i, j));
                        edgePoints.Add(new Vector3(i, j, elevation));
                    }
                }
            }

            return edgePoints;
        }

        private Vector2? CastShadowFromPoint(Vector3 startPoint, Vector2 sunDirection)
        {
            var x = startPoint.X;
            var y = startPoint.Y;
            var currentHeight = startPoint.Z;

            // Calculate the initial step increments based on the sun direction.
            var stepX = sunDirection.X;
            var stepY = sunDirection.Y;

            // Determine how far we should check for shadows based on the multiplier.
            float maxDistance = Math.Max(_mapService.Width, _mapService.Height);

            float distanceChecked = 0;
            while (distanceChecked < maxDistance)
            {
                x -= (int)stepX;
                y -= (int)stepY;

                // Break if outside heightmap boundaries.
                if (x < 0 || x >= _mapService.Width || y < 0 || y >= _mapService.Height)
                {
                    break;
                }

                float heightAtCurrentStep = _mapService.GetElevation(new Vector2(x, y));
                if (heightAtCurrentStep <= currentHeight)
                {
                    return new Vector2(x, y); // The point is in shadow.
                }

                // Increment the distance checked based on the step increments.
                distanceChecked++;
            }

            return null; // No higher terrain was found, the point is not in shadow.
        }
    }
}
