using Radler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Utils
{
    public static class VectorUtil
    {
        /// <summary>
        /// Rotate Vector around X Axis
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angles"></param>
        /// <param name="negative"></param>
        public static void RotateX(Vector vector, double angles, bool negative)
        {
            var radians = negative ? -((Math.PI / 180) * angles) : ((Math.PI / 180) * angles);

            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);

            var pX = vector.X;
            var pZ = vector.Z;

            vector.X = pX * cos - pZ * sin;
            vector.Z = pZ * cos + pX * sin;
        }

        /// <summary>
        /// Rotate Vector around Y Axis
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angles"></param>
        /// <param name="negative"></param>
        public static void RotateY(Vector vector, double angles, bool negative)
        {
            var radians = negative ? -((Math.PI / 180) * angles) : ((Math.PI / 180) * angles);

            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);

            var pX = vector.X;
            var pY = vector.Y;

            vector.X = pX * cos - pY * sin;
            vector.Y = pY * cos + pX * sin;
        }

        /// <summary>
        /// Rotate Vector Around Z Axis
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angles"></param>
        /// <param name="negative"></param>
        public static void RotateZ(Vector vector, double angles, bool negative)
        {
            var radians = negative ? -((Math.PI / 180) * angles) : ((Math.PI / 180) * angles);

            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);

            var pY = vector.Y;
            var pZ = vector.Z;

            vector.Y = pY * cos - pZ * sin;
            vector.Z = pZ * cos + pY * sin;
        }

        /// <summary>
        /// Add Vector B to Vector A
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        public static void Add(Vector a, Vector b)
        {
            a.X = a.X + b.X;
            a.Y = a.Y + b.Y;
            a.Z = a.Z + b.Z;
        }

    }
}
