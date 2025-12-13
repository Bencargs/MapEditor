using System;

namespace MapEngine.Extensions
{
    public static class EnumEx
    {
        public static T ParseOrDefault<T>(string value)
            where T : struct 
            => Enum.TryParse<T>(value, out var result) ? result : default;
    }
}
