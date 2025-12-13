using Common;
using MapEngine.Services.Map;
using System;
using System.Numerics;

namespace MapEngine.Services.Effects.LightingEffect
{
    public class ShadowEffectService
    {
        private float[] _heights;
        private byte[] _fieldOfView;
        private readonly GameTime _gameTime;
        private readonly MapService _mapService;

        public ShadowEffectService(
            GameTime gameTime,
            MapService mapService)
        {
            _gameTime = gameTime;
            _mapService = mapService;
        }

        public void Initialise()
        {
            // todo: rather than an efficient height cache here,
            // should uplift map service to make it not slow
            _heights = EnsureHeightCache();
        }

        public byte[] GenerateBitmap(Rectangle viewport)
        {
            ClearShadowBuffer(viewport);
            var (sunDirection, shadowIntensity) = GetSunDirectionVector(_gameTime.TimeOfDay);
            if (sunDirection == Vector2.Zero)
                return _fieldOfView;

            // improves shadow quality a the cost of performance - todo config
            const float shadowLength = 100f;
            const int stepLength = 2;

            // Calculate the initial step increments based on the sun direction.
            var stepX = sunDirection.X * stepLength;
            var stepY = sunDirection.Y * stepLength;
            var maxDistance = sunDirection.Length() * shadowLength;

            for (var y = 0; y < viewport.Height; y++)
            {
                for (var x = 0; x < viewport.Width; x++)
                {
                    var currentHeight = _heights[y * viewport.Width + x];

                    float sx = x;
                    float sy = y;
                    var inShadow = false;
                    var distanceChecked = 0;
                    while (distanceChecked < maxDistance)
                    {
                        sx += stepX;
                        sy += stepY;
                        distanceChecked += stepLength;

                        if (sx < 0 || sx >= viewport.Width || sy < 0 || sy >= viewport.Height)
                            break;

                        var height = _heights[(int)sy * viewport.Width + (int)sx];
                        if (height > currentHeight)
                        {
                            inShadow = true;
                            break;
                        }
                    }

                    var index = (y * viewport.Width + x) * 4;
                    _fieldOfView[index + 3] = inShadow
                        ? (byte)(shadowIntensity * 255)
                        : (byte)0;
                }
            }

            return _fieldOfView;
        }

        private static (Vector2 Direction, float Intensity) GetSunDirectionVector(TimeSpan currentTime)
        {
            // todo: from config?
            // Define key times of the day.
            var morningStart = TimeSpan.FromHours(4); // 6 AM
            var midday = TimeSpan.FromHours(12); // 12 PM
            var eveningEnd = TimeSpan.FromHours(20); // 6 PM

            // Initialize the sun direction vector.
            var sunDirection = Vector2.Zero;
            var sunIntensity = 0f;

            if (currentTime >= morningStart && currentTime <= midday)
            {
                // Interpolate from (-1, 1) at 6 AM to (0, 0) at midday.
                var progress = (float)(currentTime - morningStart).TotalHours /
                               (float)(midday - morningStart).TotalHours;
                sunDirection = new Vector2(-1, 1f - (1f * progress));
                sunIntensity = progress;
            }
            else if (currentTime > midday && currentTime <= eveningEnd)
            {
                // Interpolate from (0, 0) at midday to (1, -1) at 6 PM.
                var progress = (float)(currentTime - midday).TotalHours / (float)(eveningEnd - midday).TotalHours;
                sunDirection = new Vector2(-1, 1f * -progress);
                sunIntensity = 1f - progress;
            }
            // From 6 PM to 6 AM, sunDirection remains at (0, 0), as initialized.

            return (sunDirection, sunIntensity * 0.5f);
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

        private float[] EnsureHeightCache()
        {
            var heights = new float[_mapService.Width * _mapService.Height];
            for (var x = 0; x < _mapService.Width; x++)
            {
                for (var y = 0; y < _mapService.Height; y++)
                {
                    heights[y * _mapService.Width + x] = _mapService.GetElevation(new Vector2(x, y));
                }
            }

            return heights;
        }
    }
}
