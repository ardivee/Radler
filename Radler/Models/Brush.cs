using Radler.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Radler.Models
{
    public class Brush
    {
        public readonly ClippingPlane[] ClippingPlanes;

        // Constructor for radiant brushes.
        public Brush(ClippingPlane[] clippingPlanes)
        {
            ClippingPlanes = clippingPlanes;
        }

        // Creates a radiant brush from a piece of code.
        public static Brush CreateFromCode(string[] code, Vector offset, Vector angles)
        {
            ClippingPlane[] planes = CreateClippingPlanes(code, offset, angles);

            return new Brush(planes);
        }

        // Creates the needed clipping planes based on code.
        private static ClippingPlane[] CreateClippingPlanes(string[] code, Vector offset, Vector angles)
        {
            string num = @"-?\d+(\.\d+)?";
            string vertex = @"(\(\s?" + num + @"\s" + num + @"\s" + num + @"\s?\))";
            string pattern = vertex + @"\s?"            // First vertex [1]
                             + vertex + @"\s?"          // Second vertex [5]
                             + vertex + @"\s"           // Third vertex [9]
                             + @"(\w+(\/\S*)*)"         // Texture [13]
                             + @".*";                   // Leftovers
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            List<ClippingPlane> planes = new List<ClippingPlane>();

            foreach (string line in code)
            {
                Match m = regex.Match(line);
                if (m.Success)
                {
                    Vertex v1 = Vertex.CreateFromCode(m.Groups[1].ToString());
                    Vertex v2 = Vertex.CreateFromCode(m.Groups[5].ToString());
                    Vertex v3 = Vertex.CreateFromCode(m.Groups[9].ToString());

                    // Needs to be in this order
                    VertexUtil.RotateZ(v1, angles.Z, true);
                    VertexUtil.RotateY(v1, angles.X, false);
                    VertexUtil.RotateX(v1, angles.Y, true);

                    VertexUtil.RotateZ(v2, angles.Z, true);
                    VertexUtil.RotateY(v2, angles.X, false);
                    VertexUtil.RotateX(v2, angles.Y, true);

                    VertexUtil.RotateZ(v3, angles.Z, true);
                    VertexUtil.RotateY(v3, angles.X, false);
                    VertexUtil.RotateX(v3, angles.Y, true);

                    v1.X = v1.X + -offset.X;
                    v1.Y = v1.Y + offset.Z;
                    v1.Z = v1.Z + offset.Y;

                    v2.X = v2.X + -offset.X;
                    v2.Y = v2.Y + offset.Z;
                    v2.Z = v2.Z + offset.Y;

                    v3.X = v3.X + -offset.X;
                    v3.Y = v3.Y + offset.Z;
                    v3.Z = v3.Z + offset.Y;

                    planes.Add(new ClippingPlane(v1, v2, v3, m.Groups[13].ToString()));
                }
            }

            return planes.ToArray();
        }
    }
}
