using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Common;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
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
        /// Scene wide lighting - eg sunlight
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public byte[] ApplyDiffuseLight(Rectangle viewport)
        {
            var fieldOfView = new byte[viewport.Width * viewport.Height * 4];

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

            if (currentLight.IsTransition)
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
