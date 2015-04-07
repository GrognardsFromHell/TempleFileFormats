using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Fonts;

namespace DumpFontFace
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: DumpFontFace <.fnt file>");
                return;
            }

            var fontFace = FontFaceReader.Read(args[0]);

            using (var writer = new StreamWriter("face.log"))
            {
                for (int i = 0; i < fontFace.Glyphs.Length; ++i)
                {
                    var glyph = fontFace.Glyphs[i];
                    writer.WriteLine("[char#{0}{9}]: (x,y)=({1},{2}) width=({3}) height=({4}) width_line=({5}) width_line_x_offset=({6}) base_line_y_offset=({7}), file=({8})",
                        i, glyph.X, glyph.Y, glyph.Width, glyph.Height, glyph.WidthLine, glyph.WidthLineXOffset, glyph.BaseLineYOffset, glyph.Texture, (char)(33 + i));
                }
            }

            for (var i = 0; i < fontFace.Textures.Length; ++i)
            {
                var filename = string.Format("face{0}.png", i);
                fontFace.Textures[i].Save(filename);
            }

        }
    }
}
