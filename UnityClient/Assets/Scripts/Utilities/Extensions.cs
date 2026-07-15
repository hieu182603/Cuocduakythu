using System.Collections.Generic;

namespace CuocDuaKyThu.Utilities
{
    /// <summary>Helper extension methods used across the project.</summary>
    public static class Extensions
    {
        private static readonly System.Random Rng = new();

        /// <summary>Fisher-Yates shuffle in-place.</summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>Pick a random element from a list.</summary>
        public static T RandomElement<T>(this IList<T> list)
        {
            return list[Rng.Next(list.Count)];
        }

        /// <summary>Clamp an integer between min and max (inclusive).</summary>
        public static int Clamp(this int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
