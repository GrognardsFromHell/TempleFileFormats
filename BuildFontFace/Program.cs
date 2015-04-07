using SharpFont;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempleFileFormats.Fonts;

namespace BuildFontFace
{

    public class GlyphInfo
    {

        public char Character { get; set; }
        
        public Bitmap Bitmap { get; set; }

        public GlyphMetrics Metrics { get; set; }

        public int AdvanceX { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int TextureId { get; set; }

        public int Kerning { get; set; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("Usage: BuildFontFace <font.otf|font.ttf> <pixelSize> [<toee-font-name>]");
            }

            string fontPath = args[0];
            uint pixelSize = uint.Parse(args[1]);

            // Used to override font face name
            string toeeFontName = null;
            if (args.Length == 3)
            {
                toeeFontName = args[2];
            }
            else
            {
                toeeFontName = string.Format("{0}-{1}", Path.GetFileNameWithoutExtension(fontPath), pixelSize);
            }

            var textures = new List<Bitmap>();
            GlyphInfo[] glyphs = new GlyphInfo[188]; // ToEE builds 188 glyph starting with the exclamation mark
            for (int i = 0; i < glyphs.Length; ++i)
            {
                glyphs[i] = new GlyphInfo()
                {
                    Character = (char)('!' + i)
                };
            }

            BuildGlyphsAndAtlases(fontPath, pixelSize, textures, glyphs);

            // Save the atlas for convenience
            for (var i = 0; i < textures.Count; ++i)
            {
                textures[i].Save(string.Format("atlas{0}.png", i));
            }

            var validGlyphs = glyphs.Where(g => g.Metrics != null).ToArray();

            var face = new FontFace();
            face.Filename = toeeFontName;
            face.BaseLine = validGlyphs.Select(m => m.Metrics.HorizontalBearingY.Ceiling()).Max();
            face.LargestHeight = validGlyphs.Select(m => m.Bitmap.Height).Max();
            face.Size = (int) pixelSize;
            face.AntiAliased = true;
            face.Textures = textures.ToArray();

            // Now convert glyphs (the hard part)
            face.Glyphs = glyphs.Select(ConvertGlyph).ToArray();

            FontFaceWriter.Write(face, @".\");
        }

        private static FontFaceGlyph ConvertGlyph(GlyphInfo glyph)
        {
            var result = new FontFaceGlyph();

            result.X = glyph.X;
            result.Y = glyph.Y;
            if (glyph.Bitmap != null)
            {
                result.Width = glyph.Metrics.Width.Round();
                result.Height = glyph.Metrics.Height.Round();

                result.WidthLineXOffset = - glyph.Metrics.HorizontalBearingX.Round();
                result.WidthLine = glyph.Metrics.HorizontalAdvance.Round();
                result.BaseLineYOffset = glyph.Metrics.HorizontalBearingY.Round();
            }

            // Width Line X OFfset just seems to be ignored when drawing
            // I expected it to be subtracted from the glyphs X offset, but nope, doesnt seem to be the case... wtf
            result.X += result.WidthLineXOffset;

            return result;
        }

        private static void BuildGlyphsAndAtlases(string filename, uint pixelSize, List<Bitmap> textures, GlyphInfo[] glyphs)
        {

            using (Library lib = new Library())
            {
                using (Face face = new Face(lib, filename))
                {
                    face.SetPixelSizes(0, pixelSize);

                    var currentTexture = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                    var g = Graphics.FromImage(currentTexture);
                    var p = new Point(1, 1);
                    var lineHeight = glyphs.Select(gl => gl.Bitmap != null ? gl.Bitmap.Width : 0).Max() + 1;
                    var highestOnLine = 0;

                    foreach (var glyph in glyphs)
                    {
                        RenderGlyph(lib, face, glyph);

                        if (glyph.Metrics == null)
                            continue;

                        // Does it fit on the line?
                        if (p.X + glyph.Bitmap.Width + 1 > currentTexture.Width)
                        {
                            // Advance to the next line on the atlas
                            p.Y += highestOnLine + 1;
                            p.X = 1;
                            if (p.Y + lineHeight > currentTexture.Height)
                            {
                                p.Y = 1;
                                // Texture atlas is full
                                textures.Add(currentTexture);
                                currentTexture = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
                                /*if (textures.Count >= 4) {
                                    throw new InvalidDataException("ToEE only supports 4 texture atlases per font!");
                                }*/
                            }
                        }

                        g.DrawImageUnscaled(glyph.Bitmap, p);
                        glyph.TextureId = textures.Count;
                        glyph.X = p.X;
                        glyph.Y = p.Y;

                        p.X += glyph.Bitmap.Width + 1;
                        highestOnLine = Math.Max(highestOnLine, glyph.Bitmap.Height);
                    }
                    textures.Add(currentTexture);
                }
            }
        }

        public static void RenderGlyph(Library library, Face face, GlyphInfo glyph)
        {
            uint glyphIndex = face.GetCharIndex(glyph.Character);
            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

            face.Glyph.RenderGlyph(RenderMode.Normal);
            if (face.Glyph.Metrics.Width == 0)
            {
                return;
            }
                        
            FTBitmap ftbmp = face.Glyph.Bitmap;
            glyph.Bitmap = ftbmp.ToGdipBitmap(Color.White);
            glyph.Metrics = face.Glyph.Metrics;
            glyph.AdvanceX = face.Glyph.LinearHorizontalAdvance.Ceiling();
        }

        public static Bitmap RenderString(Library library, Face face, string text)
		{
			float penX = 0, penY = 0;
			float width = 0;
			float height = 0;

			//measure the size of the string before rendering it, requirement of Bitmap.
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				uint glyphIndex = face.GetCharIndex(c);
				face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

				width += (float)face.Glyph.Advance.X;

				if (face.HasKerning && i < text.Length - 1)
				{
					char cNext = text[i + 1];
					width += (float)face.GetKerning(glyphIndex, face.GetCharIndex(cNext), KerningMode.Default).X;
				}

				if ((float)face.Glyph.Metrics.Height > height)
					height = (float)face.Glyph.Metrics.Height;
			}

			//create a new bitmap that fits the string.
			Bitmap bmp = new Bitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);
			g.Clear(Color.FromArgb(0, 0, 0, 0));

			//draw the string
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				uint glyphIndex = face.GetCharIndex(c);
				face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
				face.Glyph.RenderGlyph(RenderMode.Normal);

				if (c == ' ')
				{
					penX += (float)face.Glyph.Advance.X;

					if (face.HasKerning && i < text.Length - 1)
					{
						char cNext = text[i + 1];
						width += (float)face.GetKerning(glyphIndex, face.GetCharIndex(cNext), KerningMode.Default).X;
					}

					penY += (float)face.Glyph.Advance.Y;
					continue;
				}

                //FTBitmap ftbmp = face.Glyph.Bitmap.Copy(library);
                FTBitmap ftbmp = face.Glyph.Bitmap;
				Bitmap cBmp = ftbmp.ToGdipBitmap(Color.Black);

				//Not using g.DrawImage because some characters come out blurry/clipped.
				//g.DrawImage(cBmp, penX + face.Glyph.BitmapLeft, penY + (bmp.Height - face.Glyph.Bitmap.Rows));
				g.DrawImageUnscaled(cBmp, (int)Math.Round(penX + face.Glyph.BitmapLeft), (int)Math.Round(penY + (bmp.Height - face.Glyph.BitmapTop)));

				penX += (float)face.Glyph.Metrics.HorizontalAdvance;
				penY += (float)face.Glyph.Advance.Y;

				if (face.HasKerning && i < text.Length - 1)
				{
					char cNext = text[i + 1];
					var kern = face.GetKerning(glyphIndex, face.GetCharIndex(cNext), KerningMode.Default);
					penX += (float)kern.X;
				}
			}

			g.Dispose();
			return bmp;
		}
    }
}
