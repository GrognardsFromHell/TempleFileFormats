using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Utils;

namespace TempleFileFormats.Fonts
{
    public static class FontFaceWriter
    {

        public static void Write(FontFace face, string path)
        {

            // Write the font face itself
            var fntPath = Path.Combine(path, face.Filename + ".fnt");
            using (var stream = new FileStream(fntPath, FileMode.Create))
            {
                var writer = new BinaryWriter(stream);

                writer.Write(face.BaseLine);
                writer.Write(face.Glyphs.Length);
                writer.Write(face.Textures.Length);
                writer.Write(face.LargestHeight);
                writer.Write(face.Size);
                writer.Write(face.AntiAliased ? 1 : 0);
                writer.WritePrefixedString(face.Filename);

                foreach (var glyph in face.Glyphs)
                {
                    writer.Write(glyph.X);
                    writer.Write(glyph.Y);
                    writer.Write(glyph.Width);
                    writer.Write(glyph.Height);
                    writer.Write(glyph.Texture);
                    writer.Write(glyph.WidthLine);
                    writer.Write(glyph.WidthLineXOffset);
                    writer.Write(glyph.BaseLineYOffset);
                }
            }

            // Write the texture atlases
            byte[] data = new byte[256 * 256];
            for (var i = 0; i < face.Textures.Length; ++i)
            {
                var texture = face.Textures[i];

                // Fontart is just a 256x256 byte array of the alpha values
                int j = 0;
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; ++x)
                    {
                        data[j++] = texture.GetPixel(x, y).A;
                    }
                }

                var fontartFilename = string.Format("{0}_{1:D4}.fntart", face.Filename, i);
                var fontartPath = Path.Combine(path, fontartFilename);
                using (var o = new FileStream(fontartPath, FileMode.Create))
                {
                    o.Write(data, 0, data.Length);
                }
            }

        }

    }
}
