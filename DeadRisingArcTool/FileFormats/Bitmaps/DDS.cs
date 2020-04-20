using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    public struct DDS_PIXELFORMAT
    {
        public const int kSizeOf = 32;

        public int dwSize;
        public int dwFlags;
        public int dwFourCC;
        public int dwRGBBitCount;
        public int dwRBitMask;
        public int dwGBitMask;
        public int dwBBitMask;
        public int dwABitMask;
    }

    [Flags]
    public enum DDSD_FLAGS
    {
        DDSD_CAPS = 0x1,
        DDSD_HEIGHT = 0x2,
        DDSD_WIDTH = 0x4,
        DDSD_PITCH = 0x8,
        DDSD_PIXELFORMAT = 0x1000,
        DDSD_MIPMAPCOUNT = 0x20000,
        DDSD_LINEARSIZE = 0x80000,
        DDSD_DEPTH = 0x800000
    }

    public struct DDS_HEADER
    {
        public const int kSizeOf = 124;
        public const int kMagic = 0x20534444;   // 'DDS '

        public int dwMagic;
        public int dwSize;      // Size of the header
        public int dwFlags;     // See DDSD_FLAGS
        public int dwHeight;
        public int dwWidth;
        public int dwPitchOrLinearSize;
        public int dwDepth;
        public int dwMipMapCount;
        public int[] dwReserved1; // 11
        public DDS_PIXELFORMAT ddspf;
        public int dwCaps;
        public int dwCaps2;
        public int dwCaps3;
        public int dwCaps4;
        public int dwReserved2;
    }

    public class DDSImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }
        public int MipMapCount { get; private set; }

        protected DDSImage()
        {

        }

        public static DDSImage FromGameTexture(rTexture texture)
        {
            // Create a new DDSImage to populate with info.
            DDSImage image = new DDSImage();

            return null;
        }
    }
}
