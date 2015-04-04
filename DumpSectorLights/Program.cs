using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Maps;
using TempleFileFormats.Objects;

namespace DumpSector
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: DumpSector <sec-filename>");
            }

            var filename = args[0];
            var sectorIo = new SectorIo(null);
            Sector sector;

            using (var reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                sector = sectorIo.ReadSector(reader);
            }

            WriteHeader("Lights");
            for (int i = 0; i < sector.Lights.Count; ++i)
            {
                var light = sector.Lights[i];
                Console.WriteLine("Light {0}", i);
                Console.WriteLine("  Pos: {0}", light.Position);
                var particles = light.Particles;
                if (particles != null && particles.ParticleSystemHash != 0)
                {
                    Console.WriteLine("  Particles: {0} (Handle: {1})", particles.ParticleSystemHash, particles.ParticleSystemHandle);
                }
                Console.WriteLine();

            }

            if (sector.Objects.Count > 0)
            {
                WriteHeader("Objects");

                foreach (var obj in sector.Objects)
                {
                    Console.WriteLine("Object {0} ({1})", obj.Id, obj.Type);
                    Console.WriteLine("  Proto ID {0}", obj.ProtoId);
                    foreach (var prop in obj.Properties)
                    {
                        Console.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                    }
                    Console.WriteLine();
                }                
            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
            
        }

        private static void WriteHeader(string name)
        {
            Console.WriteLine();
            Console.WriteLine("=====================================================================");
            Console.WriteLine(name.ToUpper());
            Console.WriteLine("=====================================================================");
            Console.WriteLine();
        }
    }
}
