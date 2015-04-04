using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Objects;
using TempleFileFormats.Utils;

namespace TempleFileFormats.Maps
{
    public class SectorIo
    {

        private readonly string mapDirectory;

        public SectorIo(string mapDirectory)
        {
            this.mapDirectory = mapDirectory;
        }

        public Sector ReadSector(int sectorX, int sectorY)
        {
            var sectorLoc = Sector.GetSectorLoc(sectorX, sectorY);

            string sectorFilename = string.Format("{0}/{1}.sec", mapDirectory, sectorLoc);

            using (var stream = new FileStream(sectorFilename, FileMode.Open))
            {
                var reader = new BinaryReader(stream);

                return ReadSector(reader);
            }

        }

        public Sector ReadSector(BinaryReader reader)
        {
            var sector = new Sector();

            ReadLights(reader, sector.Lights);

            ReadTiles(reader, sector.Tiles);

            SkipRoofList(reader);

            var placeholder = reader.ReadInt32();

            if (placeholder < 0xAA0000 || placeholder > 0xAA0004)
            {
                throw new InvalidDataException("Invalid placeholder value read from sector.");
            }

            // All of these seems to be old Arcanum leftovers
            if (placeholder >= 0xAA0001)
            {
                ReadTileScripts(reader);
            }
            if (placeholder >= 0xAA0002)
            {
                ReadSectorScripts(reader);
            }
            if (placeholder >= 0xAA0003)
            {                
                reader.ReadInt32(); // Townmap Info
                reader.ReadInt32(); // Aptitude Adjustment
                reader.ReadInt32(); // Light Scheme
                reader.ReadBytes(12); // Sound List
            }

            ReadObjects(reader, sector);
            
            return sector;
        }
        
        private void ReadLights(BinaryReader reader, IList<SectorLight> sectorLights)
        {
            var lightCount = reader.ReadInt32();

            for (var i = 0; i < lightCount; ++i)
            {
                var light = ReadLight(reader);
                sectorLights.Add(light);
            }            

        }

        private SectorLight ReadLight(BinaryReader reader)
        {
            var handle = reader.ReadUInt64();
            var type = reader.ReadInt32();
            var result = new SectorLight();

            // Read the basic light information first
            result.Handle = handle;
            result.fieldc = reader.ReadInt32();
            result.field10 = reader.ReadInt32();
            result.field14 = reader.ReadInt32();
            result.Position = reader.ReadLocation();
            result.OffsetZ = reader.ReadSingle();
            result.field_2c = reader.ReadInt32();
            result.field_30 = reader.ReadInt32();
            result.field_34 = reader.ReadInt32();
            result.field_38 = reader.ReadInt32();
            result.field_3c = reader.ReadInt32();

            if ((type & 0x10) == 0x10 || (type & 0x40) == 0x40)
            {
                var partSys = new SectorLightParticles();
                partSys.ParticleSystemHash = reader.ReadInt32();
                partSys.ParticleSystemHandle = reader.ReadInt32();
                result.Particles = partSys;
            }

            if ((type & 0x40) == 0x40)
            {
                var atNight = new SectorLightAtNight();
                atNight.field0 = reader.ReadInt32();
                atNight.field4 = reader.ReadInt32();
                atNight.field8 = reader.ReadInt32();
                atNight.fieldc = reader.ReadInt32();
                atNight.field10 = reader.ReadInt32();
                atNight.field14 = reader.ReadInt32();
                atNight.field18 = reader.ReadInt32();
                result.AtNight = atNight;

                var partSys = new SectorLightParticles();
                partSys.ParticleSystemHash = reader.ReadInt32();
                partSys.ParticleSystemHandle = reader.ReadInt32();
                atNight.Particles = partSys;
            }

            return result;
        }

        private void ReadTiles(BinaryReader reader, SectorTile[] tiles)
        {
            for (int i = 0; i < tiles.Length; ++i)
            {
                tiles[i].Data = reader.ReadBytes(16);
            }
        }

        private void SkipRoofList(BinaryReader reader)
        {
            var isPresent = reader.ReadInt32();
            if (isPresent == 0)
            {
                reader.BaseStream.Seek(256 * 4, SeekOrigin.Current);
            }
        }

        private void ReadTileScripts(BinaryReader reader)
        {
            var count = reader.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                var tileScript = reader.ReadBytes(24);
                Console.WriteLine("Tile Script Read!");
            }
        }


        private void ReadSectorScripts(BinaryReader reader)
        {
            reader.ReadBytes(12);
        }

        private void ReadObjects(BinaryReader reader, Sector sector)
        {
            /*
             * The last 4 byte of the sector file contain the object count.
             */
            var stream = reader.BaseStream;
            var startOfObjects = stream.Position;
            stream.Seek(-4, SeekOrigin.End);
            var count = reader.ReadInt32();
            stream.Seek(startOfObjects, SeekOrigin.Begin);

            var objReader = new GameObjectReader(reader);

            for (var i = 0; i < count; ++i)
            {
                sector.Objects.Add(objReader.Read());
            }
        }
                
    }
}
