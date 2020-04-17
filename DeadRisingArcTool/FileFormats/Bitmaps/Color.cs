using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    internal class Color
    {

        public struct ColorRGBA
        {
            public byte a, r, g, b;
        }

        public static ColorRGBA ShortToColor(int color)
        {
            return ShortToColor(Convert.ToUInt16(color));
        }
        public static ColorRGBA ShortToColor(ushort color)
        {
            ColorRGBA rcs;
            rcs.r = (byte)((((color >> 11) & 31) * 255) / 31);
            rcs.g = (byte)((((color >> 5) & 63) * 255) / 63);
            rcs.b = (byte)((((color >> 0) & 31) * 255) / 31);
            rcs.a = 255;
            return rcs;
        }

        public static int ColorToInt(ColorRGBA rcs)
        {
            return (rcs.a << 24) | (rcs.r << 16) | (rcs.g << 8) | rcs.b;
        }

        public static ColorRGBA IntToRGBA(uint color)
        {
            ColorRGBA rc;
            rc.r = (byte)((((color >> 11) & 31) * 255) / 31);
            rc.g = (byte)((((color >> 5) & 63) * 255) / 63);
            rc.b = (byte)((((color >> 0) & 31) * 255) / 31);
            rc.a = 255;
            return rc;
        }

        public static ColorRGBA GradientColors(ColorRGBA Col1, ColorRGBA Col2)
        {
            ColorRGBA ret;
            ret.r = (byte)(((Col1.r * 2 + Col2.r)) / 3);
            ret.g = (byte)(((Col1.g * 2 + Col2.g)) / 3);
            ret.b = (byte)(((Col1.b * 2 + Col2.b)) / 3);
            ret.a = 255;
            return ret;
        }

        public static ColorRGBA GradientColorsHalf(ColorRGBA Col1, ColorRGBA Col2)
        {
            ColorRGBA ret;
            ret.r = (byte)(Col1.r / 2 + Col2.r / 2);
            ret.g = (byte)(Col1.g / 2 + Col2.g / 2);
            ret.b = (byte)(Col1.b / 2 + Col2.b / 2);
            ret.a = 255;
            return ret;
        }
    }
}
