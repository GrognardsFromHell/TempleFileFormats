using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Utils;

namespace TempleFileFormats.Fonts
{
    public static class FontFaceReader
    {

        public static FontFace Read(string filename)
        {
            var result = new FontFace();
            int numberOfFiles;
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var reader = new BinaryReader(stream);
                result.BaseLine = reader.ReadInt32();
                var glyphCount = reader.ReadInt32();
                numberOfFiles = reader.ReadInt32();
                result.LargestHeight = reader.ReadInt32();
                result.Size = reader.ReadInt32();
                result.AntiAliased = (reader.ReadInt32() == 1);
                result.Filename = reader.ReadPrefixedString();

                var glyphs = new FontFaceGlyph[glyphCount];
                for (int i = 0; i < glyphCount; ++i)
                {
                    glyphs[i] = ReadGlyph(reader);
                }
                result.Glyphs = glyphs;
            }

            var textures = new Bitmap[numberOfFiles];
            var dir = Path.GetDirectoryName(filename);
            var buffer = new byte[256*256];
            for (var i = 0; i < numberOfFiles; ++i)
            {
                var fntArtFile = Path.Combine(dir, string.Format("{0}_{1:D4}.fntart", result.Filename, i));
                using (var stream = new FileStream(fntArtFile, FileMode.Open))
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    Debug.Assert(read == buffer.Length);
                }

                var texture = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                var j = 0;
                
                for (var y = 0; y < 256; ++y)
                {
                    for (var x = 0; x < 256; ++x)
                    {
                        var alpha = buffer[j++];
                        texture.SetPixel(x, y, Color.FromArgb(alpha, 255, 255, 255));
                    }
                }
                textures[i] = texture;
            }
            result.Textures = textures;

            return result;
        }

        private static FontFaceGlyph ReadGlyph(BinaryReader reader)
        {
            var glyph = new FontFaceGlyph();            
            glyph.X = reader.ReadInt32();
            glyph.Y = reader.ReadInt32();
            glyph.Width = reader.ReadInt32();
            glyph.Height = reader.ReadInt32();
            glyph.Texture = reader.ReadInt32();
            glyph.WidthLine = reader.ReadInt32();
            glyph.WidthLineXOffset = reader.ReadInt32();
            glyph.BaseLineYOffset = reader.ReadInt32();
            return glyph;
        }

    }
}
