using Radler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Utils
{
    public static class VertexUtil
    {
        /// <summary>
        /// Rotate Vertex Around X Axis
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="angles"></param>
        /// <param name="negative"></param>
        public static void RotateX(Vertex vertex, double angles, bool negative)
        {
            var radians = negative ? -((Math.PI / 180) * angles) : ((Math.PI / 180) * angles);

            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);

            var pX = vertex.X;
            var pZ = vertex.Z;

            vertex.X = pX * cos - pZ * sin;
            vertex.Z = pZ * cos + pX * sin;
        }

        /// <summary>
        /// Rotate Vertex Around Y Axis
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="angles"></param>
        /// <param name="negative"></param>
        public static void RotateY(Vertex vertex, double angles, bool negative)
        {
            var radians = negative ? -((Math.PI / 180) * angles) : ((Math.PI / 180) * angles);

            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);

            var pX = vertex.X;
            var pY = vertex.Y;

            vertex.X = pX * cos - pY * sin;
            vertex.Y = pY * cos + pX * sin;
        }

        /// <summary>
        /// Rotate Vertex Around Z Axis
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="angles"></param>
        /// <param name="negative"></param>
        public static void RotateZ(Vertex vertex, double angles, bool negative)
        {
            var radians = negative ? -((Math.PI / 180) * angles) : ((Math.PI / 180) * angles);

            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);

            var pY = vertex.Y;
            var pZ = vertex.Z;

            vertex.Y = pY * cos - pZ * sin;
            vertex.Z = pZ * cos + pY * sin;
        }

        /// <summary>
        /// Add Vertex B to Vertex A
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="xNegative"></param>
        /// <param name="yNegative"></param>
        /// <param name="zNegative"></param>
        public static void Add(Vertex a, Vertex b, bool xNegative = false, bool yNegative = false, bool zNegative = false)
        {
            a.X = a.X + (xNegative ? -b.X : b.X);
            a.Y = a.Y + (yNegative ? -b.Y : b.Y);
            a.Z = a.Z + (zNegative ? -b.Z : b.Z);
        }

        /// <summary>
        /// Add Vector B to Vertex A
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="xNegative"></param>
        /// <param name="yNegative"></param>
        /// <param name="zNegative"></param>
        public static void Add(Vertex a, Vector b, bool xNegative = false, bool yNegative = false, bool zNegative = false)
        {
            a.X = a.X + (xNegative ? -b.X : b.X);
            a.Y = a.Y + (yNegative ? -b.Y : b.Y);
            a.Z = a.Z + (zNegative ? -b.Z : b.Z);
        }
    }
}
