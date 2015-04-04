using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Common
{
    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }

        public override string ToString()
        {
            return String.Format("{0},{1} ({2},{3})", X, Y, OffsetX, OffsetY);
        }
    }
}
