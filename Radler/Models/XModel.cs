using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Models
{
    public class XModel
    {
        public string Name { get; set; }
        public Vector Origin { get; set; }

        public Vector Angles { get; set; }

        public string FilePath { get; set; }

        public XModel()
        {
            Name = "";
            Origin = new Vector(0, 0, 0);
            Angles = new Vector(0, 0, 0);
            FilePath = "";
        }
    }
}
