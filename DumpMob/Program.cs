using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Objects;

namespace DumpMob
{
    class Program
    {

        private static int mobsRead = 0;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: DumpMob <mob-filename|directory>");
            }

            var filename = args[0];

            using (var w = new StreamWriter("mob.log", false, Encoding.UTF8, 8192))
            {
                if (Directory.Exists(filename))
                {
                    DumpAllIn(filename, w);
                }
                else
                {
                    DumpFile(filename, w);
                }
            }

            Console.WriteLine("Done. Written {0} mobs to mob.log.", mobsRead);
            Console.ReadKey();

        }

        private static void DumpAllIn(string filename, StreamWriter w)
        {
            foreach (var file in Directory.EnumerateFiles(filename, "*.mob", SearchOption.AllDirectories))
            {
                DumpFile(file, w);
            }
        }

        private static void DumpFile(string filename, StreamWriter w)
        {
            WriteHeader(filename, w);

            mobsRead++;

            GameObject obj;
            using (var reader = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                obj = new GameObjectReader(reader).Read();
            }

            w.WriteLine("{1} {0}", obj.Id, obj.Type);
            w.WriteLine("  Proto ID {0}", obj.ProtoId);
            foreach (var prop in obj.Properties)
            {
                if (prop.Value is IEnumerable)
                {
                    IEnumerable e = prop.Value as IEnumerable;
                    List<string> vals = new List<string>();
                    foreach (var k in e)
                    {
                        vals.Add(k == null ? "null" : k.ToString());
                    }
                    w.WriteLine("  {0}: {1}", prop.Key, string.Join(", ", vals));
                }
                else
                {
                    w.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                }

            }
            w.WriteLine();
        }

        private static void WriteHeader(string name, StreamWriter w)
        {
            w.WriteLine();
            w.WriteLine("=====================================================================");
            w.WriteLine(name.ToUpper());
            w.WriteLine("=====================================================================");
            w.WriteLine();
        }
    }
}
