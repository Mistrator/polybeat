using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Polybeat
{
    /// <summary>
    /// Satunnaisgeneraattoreita eri tietotyypeille.
    /// </summary>
    public static class RandomGen
    {
        private static Random r = new Random();

        public static float NextFloat(float min, float max)
        {
            return (float)(min + r.NextDouble() * (max - min));
        }

        public static double NextDouble(double min, double max)
        {
            return min + r.NextDouble() * (max - min);
        }

        /// <summary>
        /// Palauttaa satunnaisen kokonaisluvun.
        /// </summary>
        /// <param name="min">Pienin arvo, inclusive</param>
        /// <param name="max">Suurin arvo, exclusive</param>
        /// <returns></returns>
        public static int NextInt(int min, int max)
        {
            return r.Next(min, max);
        }

        public static Vector2 NextVector(Vector2 min, Vector2 max)
        {
            return new Vector2(NextFloat(min.X, max.X), NextFloat(min.Y, max.Y));
        }

        public static Vector2 NextVector(float min, float max)
        {
            return new Vector2(RandomGen.NextFloat(min, max), RandomGen.NextFloat(min, max));
        }
    }
}