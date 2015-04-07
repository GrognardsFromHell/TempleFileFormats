using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempleFileFormats.Fonts
{
    public class FontFace
    {

        /// <summary>
        /// Height of the original font face in pixels i think.
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Basename of the font face. Used to load fntart files.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Height of the tallest glyph in pixels.
        /// </summary>
        public int LargestHeight { get; set; }

        /// <summary>
        /// The glyphs included in this font.
        /// </summary>
        public FontFaceGlyph[] Glyphs { get; set; }

        /// <summary>
        /// Was the font rendered with anti-aliasing?
        /// </summary>
        public bool AntiAliased { get; set; }

        public int BaseLine { get; set; }

        /// <summary>
        /// The 256x256 alpha channel texture that contains the glyphs.
        /// </summary>
        public Bitmap[] Textures { get; set; }
    }

    public class FontFaceGlyph
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// Which texture (index) this glyph is on.
        /// </summary>
        public int Texture { get; set; }
        public int WidthLine { get; set; }
        public int WidthLineXOffset { get; set; }
        public int BaseLineYOffset { get; set; }
    }

}
