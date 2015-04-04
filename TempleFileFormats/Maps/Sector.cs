using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Common;
using TempleFileFormats.Objects;

namespace TempleFileFormats.Maps
{
    public class Sector
    {

        public IList<SectorLight> Lights { get; private set; }

        public SectorTile[] Tiles { get; private set; }

        public List<GameObject> Objects { get; private set; }

        public Sector()
        {
            Lights = new List<SectorLight>();
            Tiles = new SectorTile[4096];
            Objects = new List<GameObject>();
        }        

        public static uint GetSectorLoc(int x, int y)
        {
            return ((uint) y << 26) & 0xFC | ((uint) x & 0xFC);
        }

    }

    public struct SectorTile
    {
        public byte[] Data { get; set; }
    }

    public class SectorLight
    {
        public ulong Handle { get; set; }
        public int fieldc { get; set; }
        public int field10 { get; set; }
        public int field14 { get; set; }
        public Location Position { get; set; } // x, y, offsx, offsy
        public float OffsetZ { get; set; } // Speculation
        public int field_2c { get; set; }
        public int field_30 { get; set; }
        public int field_34 { get; set; }
        public int field_38 { get; set; }
        public int field_3c { get; set; }
        public SectorLightParticles Particles { get; set; }
        public SectorLightAtNight AtNight { get; set; }
    }

    public class SectorLightParticles {
        public int ParticleSystemHash { get; set; } // Hash of the particle system name. Can be 0 to disable particles.
        public int ParticleSystemHandle { get; set; } // Should be 0. Is set at runtime
    };

    public class SectorLightAtNight {
        public int field0 { get; set; }
        public int field4 { get; set; }
        public int field8 { get; set; }
        public int fieldc { get; set; }
        public int field10 { get; set; }
        public int field14 { get; set; }
        public int field18 { get; set; }
        public SectorLightParticles Particles { get; set; }
    };
   
}
