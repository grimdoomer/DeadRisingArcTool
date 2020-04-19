using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    public class MaskSet
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;

        public MaskSet(int w, int h, int d)
        {
            int bit = 1;
            int index = 1;

            while (bit < w || bit < h || bit < d)
            {
                //if (bit == 0) break;
                if (bit < w)
                {
                    x |= index;
                    index <<= 1;
                }
                if (bit < h)
                {
                    y |= index;
                    index <<= 1;
                }
                if (bit < d)
                {
                    z |= index;
                    index <<= 1;
                }
                bit <<= 1;
            }
        }
    }

    public class Swizzle
    {
        public static byte[] ConvertToLinearTexture(byte[] data, int width, int height, TextureFormat texture)
        {
            return ModifyLinearTexture(data, width, height, texture, true);
        }

        public static byte[] ConvertFromLinearTexture(byte[] data, int width, int height, TextureFormat texture)
        {
            return ModifyLinearTexture(data, width, height, texture, false);
        }

        private static byte[] ModifyLinearTexture(byte[] data, int width, int height, TextureFormat texture, bool toLinear)
        {
            // Set up our destination buffer
            byte[] destData = new byte[data.Length];

            // Figure out our block size and texel pitch
            int blockSizeRow, blockSizeColumn;
            int texelPitch;
            switch (texture)
            {
                case TextureFormat.Format_DXT1:
                    blockSizeRow = 4;
                    blockSizeColumn = 4;
                    texelPitch = 8;
                    break;
                case TextureFormat.Format_DXT2:
                case TextureFormat.Format_DXT5:
                    blockSizeRow = 4;
                    blockSizeColumn = 4;
                    texelPitch = 16;
                    break;
                //case TextureFormat.AY8:
                //    blockSizeRow = 4;
                //    blockSizeColumn = 4;
                //    texelPitch = 2;
                //    break;
                case TextureFormat.Format_B8G8R8A8_UNORM:
                default:
                    blockSizeRow = 1;
                    blockSizeColumn = 1;
                    texelPitch = 2;
                    break;
                    //throw new ArgumentOutOfRangeException("Bad type!");
            }

            // Figure out our block height and width
            int blockWidth = width / blockSizeRow;
            int blockHeight = height / blockSizeColumn;

            // Loop through the height and width and copy our data
            //try
            {
                for (int j = 0; j < blockHeight; j++)
                    for (int i = 0; i < blockWidth; i++)
                    {
                        int blockOffset = j * blockWidth + i;

                        int x = XGAddress2DTiledX(blockOffset, blockWidth, texelPitch);
                        int y = XGAddress2DTiledY(blockOffset, blockWidth, texelPitch);

                        int srcOffset = j * blockWidth * texelPitch + i * texelPitch;
                        int destOffset = y * blockWidth * texelPitch + x * texelPitch;

                        if (toLinear)
                            Array.Copy(data, srcOffset, destData, destOffset, texelPitch);
                        else
                            Array.Copy(data, destOffset, destData, srcOffset, texelPitch);
                    }
            }
            //catch
            //{ }

            return destData;
        }

        private static int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroX = ((OffsetM % (AlignedWidth >> 5)) << 2);
            int Tile = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
            int Macro = (MacroX + Tile) << 3;
            int Micro = ((((OffsetT >> 1) & ~15) + (OffsetT & 15)) & ((TexelPitch << 3) - 1)) >> LogBpp;

            return Macro + Micro;
        }

        private static int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroY = ((OffsetM / (AlignedWidth >> 5)) << 2);
            int Tile = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
            int Macro = (MacroY + Tile) << 3;
            int Micro = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 15) << 1)) >> (3 + LogBpp)) & ~1);

            return Macro + Micro + ((OffsetT & 16) >> 4);
        }







        public static byte[] SwizzleData(byte[] buffer, int Width, int Height, int depth, int bitCount, bool deswizzle)
        {
            bitCount /= 8;
            int a = 0;
            int b = 0;
            byte[] dataArray = new byte[buffer.Length]; //Bitmap.Width * Height * bitCount;

            MaskSet masks = new MaskSet(Width, Height, depth);
            int pixOffset = 0;
            for (int y = 0; y < Height * depth; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (deswizzle)
                    {
                        a = ((y * Width) + x) * bitCount;
                        b = (SwizzleData(x, y, depth, masks)) * bitCount;
                    }
                    else
                    {
                        b = ((y * Width) + x) * bitCount;
                        a = (SwizzleData(x, y, depth, masks)) * bitCount;
                    }

                    if (a < dataArray.Length && b < buffer.Length)
                    {
                        for (int i = pixOffset; i < bitCount + pixOffset; i++)
                            dataArray[a + i] = buffer[b + i];
                    }
                    else return null;
                }
            }

            //for (int u = 0; u < pixOffset; u++)
            //    raw[u] = raw[u];
            //for (int v = pixOffset + (Height * Width * depth * bitCount); v < raw.Length; v++)
            //    raw[v] = raw[v];

            return dataArray;
        }

        public static int SwizzleData(int x, int y, int z, MaskSet masks)
        {
            return SwizzleAxis(x, masks.x) | SwizzleAxis(y, masks.y) | (z == -1 ? 0 : SwizzleAxis(z, masks.z));
        }

        public static int SwizzleAxis(int val, int mask)
        {
            int bit = 1;
            int result = 0;

            while (bit <= mask)
            {
                int tmp = mask & bit;

                if (tmp != 0) result |= (val & bit);
                else val <<= 1;

                bit <<= 1;
            }

            return result;
        }
    }
}
