using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEngine.ResourceLoading
{
    public class Effect
    {
        public string[] Particles { get; set; }
    }

    public static class EffectsLoader
    {
        public static Effect LoadEffect(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic effectData = JsonConvert.DeserializeObject(json);

            var particles = ToStringArray(effectData.Particles);

            return new Effect
            {
                Particles = particles
            };
        }

        private static string[] ToStringArray(dynamic property)
        {
            return ((IEnumerable<dynamic>)property).Select(x => (string)x).ToArray();
        }
    }
}
