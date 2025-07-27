using System.Windows;
using System.Windows.Media;

namespace Translator.Palette
{
    public static class PaletteExtension
    {
        /// <summary>
        /// If resource dictionary contains a key
        /// </summary>
        /// <param name="resourceDictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(this ResourceDictionary resourceDictionary, string key)
        {
            return resourceDictionary != null && resourceDictionary.Contains(key);
        }

        /// <summary>
        /// If resource dictionary contains a key, returns value as type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceDictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this ResourceDictionary resourceDictionary, string key, out T value)
            where T : class
        {
            value = null;
            if (resourceDictionary.HasKey(key))
                value = resourceDictionary[key] as T;
            return value != null;
        }

        /// <summary>
        /// Compares two System.Windows.Media.Colors
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool DiffersFrom(this Color current, Color target)
        {
            return current != target;
        }
    }
}
