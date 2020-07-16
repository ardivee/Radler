using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Models
{
    public class Prefab
    {
        public string Path { get; set; }
        public Vector Origin { get; set; }

        public Vector Angles { get; set; }

        public Prefab()
        {
            Origin = new Vector(0, 0, 0);
            Angles = new Vector(0, 0, 0);
        }

        public Prefab(string path, Vector offset)
        {
            Path = path;
            Origin = offset;
        }
    }
}
