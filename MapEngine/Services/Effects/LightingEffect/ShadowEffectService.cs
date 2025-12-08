using Common;
using MapEngine.Services.Map;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MapEngine.Services.Effects.LightingEffect
{
    public class ShadowEffectService
    {
        private byte[] _fieldOfView;
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
            ClearShadowBuffer(viewport);
            var (sunDirection, shadowIntensity) = GetSunDirectionVector(_gameTime.TimeOfDay);
            if (sunDirection == Vector2.Zero)
                return _fieldOfView;

            // Find all the edges of regions on the heightmap - reduces processing vs checking every pixel
            var shadows = new HashSet<int>();
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
                    var (x, y) = ((int)shadowPoint.X, (int)shadowPoint.Y);
                    int key = y * viewport.Width + x;
                    if (!shadows.Add(key))
                        continue;

                    // only draw a shadow if it is projected below the terrains height
                    var shadowHeight = _mapService.GetElevation(new Vector2(x, y));
                    if (shadowHeight > edge.Z)
                        continue;

                    var index = (int)(shadowPoint.Y * viewport.Width + shadowPoint.X) * 4;
                    _fieldOfView[index + 3] = (byte)(_fieldOfView[index + 3] + (148 * shadowIntensity));
                }
            }

            return _fieldOfView;
        }

        private void ClearShadowBuffer(Rectangle viewport)
        {
            if (_fieldOfView == null)
            {
                _fieldOfView = new byte[viewport.Width * viewport.Height * 4];
                return;
            }
            
            Array.Clear(_fieldOfView, 0, viewport.Width * viewport.Height * 4);
        }

        private static (Vector2 Direction, float Intensity) GetSunDirectionVector(TimeSpan currentTime)
        {
            // todo: from config?
            // Define key times of the day.
            var morningStart = TimeSpan.FromHours(4);  // 6 AM
            var midday = TimeSpan.FromHours(12);       // 12 PM
            var eveningEnd = TimeSpan.FromHours(20);   // 6 PM

            // Initialize the sun direction vector.
            var sunDirection = Vector2.Zero;
            var sunIntensity = 0f;
            var shadowLength = 100;

            if (currentTime >= morningStart && currentTime <= midday)
            {
                // Interpolate from (-1, 1) at 6 AM to (0, 0) at midday.
                var progress = (float)(currentTime - morningStart).TotalHours / (float)(midday - morningStart).TotalHours;
                sunDirection = new Vector2(-shadowLength, shadowLength - (shadowLength * progress));
                sunIntensity = progress;
            }
            else if (currentTime > midday && currentTime <= eveningEnd)
            {
                // Interpolate from (0, 0) at midday to (1, -1) at 6 PM.
                var progress = (float)(currentTime - midday).TotalHours / (float)(eveningEnd - midday).TotalHours;
                sunDirection = new Vector2(-shadowLength, shadowLength * -progress);
                sunIntensity = 1f - progress;
            }
            // From 6 PM to 6 AM, sunDirection remains at (0, 0), as initialized.

            return (sunDirection, sunIntensity);
        }

        //todo: third time, please refactor

        // Bresenham's line algorithm variables
        private static List<Vector2> DrawShadowRay(Vector3 start, Vector2 end)
        {
            var (startX, startY, endX, endY) = ((int)start.X, (int)start.Y, (int)end.X, (int)end.Y);
            int dx = Math.Abs(endX - startX), sx = startX < endX ? 1 : -1;
            int dy = -Math.Abs(endY - startY), sy = startY < endY ? 1 : -1;
            int err = dx + dy;

            var shadowRayPoints = new List<Vector2>();
            while (true)
            {
                if (startX == endX && startY == endY) break;

                var e2 = 2 * err;
                if (e2 >= dy)
                {
                    if (startX == endX) break;
                    err += dy; startX += sx;
                }
                if (e2 <= dx)
                {
                    if (startY == endY) break;
                    err += dx; startY += sy;
                }
                shadowRayPoints.Add(new Vector2(startX, startY));
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
            var x = (int)startPoint.X;
            var y = (int)startPoint.Y;
            var currentHeight = startPoint.Z;

            // Calculate the initial step increments based on the sun direction.
            var stepX = (int)sunDirection.X;
            var stepY = (int)sunDirection.Y;

            // Determine how far we should check for shadows based on the multiplier.
            var maxDistance = Math.Max(_mapService.Width, _mapService.Height);

            var distanceChecked = 0;
            while (distanceChecked++ < maxDistance)
            {
                x -= stepX;
                y -= stepY;

                // Break if outside heightmap boundaries.
                if (x < 0 || x >= _mapService.Width || y < 0 || y >= _mapService.Height)
                    break;

                var point = new Vector2(x, y);
                var heightAtCurrentStep = _mapService.GetElevation(point);
                if (heightAtCurrentStep <= currentHeight)
                    return point; // The point is in shadow.
            }

            return null; // No higher terrain was found, the point is not in shadow.
        }
    }
}
