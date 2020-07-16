using Radler.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Radler.Models
{
    public class Patch
    {
        public Vertex[][] Grid { get; private set; }

        private int _x;
        private int _y;

        public Vertex[] this[int index]
        {
            get => Grid[index];
            set => Grid[index] = value;
        }

        // Constructor for a patch.
        public Patch(int width, int height)
        {
            Grid = new Vertex[width][];

            for (int i = 0; i < Grid.Length; ++i)
            {
                Grid[i] = new Vertex[height];
                for (int j = 0; j < Grid[i].Length; ++j)
                    Grid[i][j] = null;
            }
        }

        // Collect all vertices and return them.
        public Vertex[] GetVertices()
        {
            List<Vertex> vertices = new List<Vertex>();

            foreach (Vertex[] row in Grid)
            {
                foreach (Vertex vert in row)
                    vertices.Add(vert);
            }

            return vertices.ToArray();
        }

        // Adds a vertex to the grid in the next available slot.
        public void Add(Vertex vertex)
        {
            Grid[_y][_x] = vertex;

            if (_x < Grid[0].Length - 1)
                _x++;
            else
            {
                _x = 0;
                if (_y < Grid.Length - 1)
                    _y++;
                else
                    _y = 0;
            }
        }

        // Creates a radiant patch from a piece of code.
        public static Patch CreateFromCode(string[] code, Vector offset, Vector angles)
        {
            string sizePattern = @"(\s+)?\s?(\d+)\s(\d+)(\s(\d+)\s(\d+))";

            Regex sizeRegex = new Regex(sizePattern, RegexOptions.IgnoreCase);

            Patch patch = null;

            int line = 0;

            while (line < code.Length)
            {
                Match m = sizeRegex.Match(code[line]);
                ++line;
                if (m.Success)
                {
                    //Console.WriteLine(string.Format("Patch Width: {0} || Patch Height: {1}", int.Parse(m.Groups[2].ToString()), int.Parse(m.Groups[3].ToString())));
                    patch = new Patch(int.Parse(m.Groups[2].ToString()), int.Parse(m.Groups[3].ToString()));
                    break;
                }
            }

            bool readVertex = false;
            while (line < code.Length)
            {
                if (code[line].Contains(")"))
                {
                    readVertex = false;
                }

                if (readVertex)
                {
                    string lineTrimStart = code[line].TrimStart();

                    if(lineTrimStart.StartsWith("v "))
                    {
                        string vline = code[line].TrimStart();
                        string[] v_split = vline.Split(' ');
                        //Console.WriteLine(string.Format("X: {0}, Y: {1}, Z: {2}", v_split[1], v_split[2], v_split[3]));

                        // Swap the Y and Z
                        Vertex v = new Vertex(
                            Double.Parse(v_split[1].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                            -Double.Parse(v_split[3].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                            -Double.Parse(v_split[2].ToString(), System.Globalization.CultureInfo.InvariantCulture));

                        //Console.WriteLine(string.Format("Vertex X: {0}, Y {1}, Z {2}", -Double.Parse(m[i].Groups[2].ToString(), System.Globalization.CultureInfo.InvariantCulture), -Double.Parse(m[i].Groups[4].ToString(), System.Globalization.CultureInfo.InvariantCulture), -Double.Parse(m[i].Groups[6].ToString(), System.Globalization.CultureInfo.InvariantCulture)));

                        // Z Y X order else we fail lol ...
                        VertexUtil.RotateZ(v, angles.Z, true);
                        VertexUtil.RotateY(v, angles.X, false);
                        VertexUtil.RotateX(v, angles.Y, true);

                        // Set offset after
                        v.X = v.X + offset.X;
                        v.Y = v.Y + -offset.Z;
                        v.Z = v.Z + -offset.Y;

                        if (patch != null)
                            patch.Add(v);
                    }  
                }

                if (code[line].Contains("("))
                    readVertex = true;

                line++;
            }

            return patch;
        }
    }
}
