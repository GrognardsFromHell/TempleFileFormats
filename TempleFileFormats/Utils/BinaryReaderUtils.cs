using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Utils
{
    static class BinaryReaderUtils
    {
        /// <summary>
        /// Reads a string from the stream that is prefixed with a 32-bit integer containing its length. The
        /// string is assumed to not be null terminated. The string is decoded using the default encoding for
        /// this platform.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>The string read from the reader.</returns>
        public static string ReadPrefixedString(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);

            // Decode using local encoding
            return Encoding.Default.GetString(data);
        }

    }
}
