using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.SaveGames.Archive;

namespace CompressSaveGame
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: CompressSaveGame <dir> <savename>");
                return;
            }

            var dir = args[0];
            var name = args[1];

            var tfaiName = name + ".tfai";
            var tfafName = name + ".tfaf";

            ArchiveWriter.Compress(tfaiName, tfafName, dir);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
    }
}
