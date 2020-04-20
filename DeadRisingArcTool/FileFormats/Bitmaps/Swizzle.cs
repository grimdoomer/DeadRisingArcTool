using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public enum XGTILE : int
        {
            NONE = 0x0,
            XGTILE_NONPACKED = 0x1,
            XGTILE_BORDER = 0x2
        }

        public class XGPOINT
        {
            public int X;
            public int Y;
            public int Z;
        }

        public class D3DBOX
        {
            public uint Left;
            public uint Top;
            public uint Right;
            public uint Bottom;
            public uint Front;
            public uint Back;
        }

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

            //for (int y = 0; y < blockHeight; y++)
            //{
            //    for (int x = 0; x < blockWidth; x++)
            //    {
            //        int 
            //    }
            //}

            // Loop through the height and width and copy our data
            //try
            //{
            for (int j = 0; j < blockHeight; j++)
                for (int i = 0; i < blockWidth; i++)
                {
                    int blockOffset = j * blockWidth + i;

                    int x = XGAddress2DTiledX(blockOffset, blockWidth, texelPitch);
                    int y = XGAddress2DTiledY(blockOffset, blockWidth, texelPitch);

                    int srcOffset = (j * blockWidth + i) * texelPitch;
                    int destOffset = (y * blockWidth + x) * texelPitch;

                    if (toLinear)
                        Array.Copy(data, srcOffset, destData, destOffset, texelPitch);
                    else
                        Array.Copy(data, destOffset, destData, srcOffset, texelPitch);
                }
        //}
            //catch
            //{ }

            //return UntileSurface((uint)blockWidth, (uint)blockHeight, (uint)blockWidth, new Point(0, 0), data, (uint)texelPitch, new Rectangle(0, 0, blockWidth, blockHeight));

            return destData;
        }

        int XGAddress2DTiledOffset(
        int x,             // x coordinate of the texel/block
        int y,             // y coordinate of the texel/block
        int Width,         // Width of the image in texels/blocks
        int TexelPitch     // Size of an image texel/block in bytes
        )
            {
            int AlignedWidth;
            int LogBpp;
            int Macro;
            int Micro;
            int Offset;

                AlignedWidth = (Width + 31) & ~31;
                LogBpp = (int)XGLog2LE16((uint)TexelPitch);
                Macro = ((x >> 5) + (y >> 5) * (AlignedWidth >> 5)) << (LogBpp + 7);
                Micro = (((x & 7) + ((y & 6) << 2)) << LogBpp);
                Offset = Macro + ((Micro & ~15) << 1) + (Micro & 15) + ((y & 8) << (3 + LogBpp)) + ((y & 1) << 4);

                return (((Offset & ~511) << 3) + ((Offset & 448) << 2) + (Offset & 63) +
                        ((y & 16) << 7) + (((((y & 8) >> 2) + (x >> 3)) & 3) << 6)) >> LogBpp;
            }















        public static uint XGBitsPerPixelFromGpuFormat(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Format_DXT1: return 4;
                case TextureFormat.Format_DXT2:
                case TextureFormat.Format_DXT5: return 8;
                default: return 0;
            }
        }

        /// <summary>
        /// Get the dimension of the compression block, if the format is not compressed, return 1x1 blocks
        /// </summary>
        /// <param name="format"></param>
        /// <param name="pWidth"></param>
        /// <param name="pHeight"></param>
        /// <returns></returns>
        public static uint XGGetBlockDimensions(TextureFormat format, out uint pWidth, out uint pHeight)
        {
            uint result;

            switch (format)
            {
                case TextureFormat.Format_DXT1:
                case TextureFormat.Format_DXT2:
                case TextureFormat.Format_DXT5:
                    result = 4;
                    pWidth = result;
                    pHeight = result;
                    break;

                default:
                    result = 1;
                    pWidth = 1;
                    pHeight = 1;
                    break;
            }
            return result;
        }

        public static uint D3DGetMipTailLevelOffsetCoords(uint level, uint width, uint height, uint depth, uint slicePitch, uint size, TextureFormat format, ref uint offsetX, ref uint offsetY, ref uint offsetZ)
        {
            offsetX = 0;
            offsetY = 0;
            offsetZ = 0;
            uint blockWidth = 0;
            uint blockHeight = 0;

            uint bitsPerPixel = XGBitsPerPixelFromGpuFormat(format);
            XGGetBlockDimensions(format, out blockWidth, out blockHeight);

            uint logWidth = Log2Ceiling((int)(width - 1));
            uint logHeight = Log2Ceiling((int)(height - 1));
            uint logDepth = Log2Ceiling((int)(depth - 1));

            uint nextPowerTwoWidth = (uint)1 << (int)logWidth;
            uint nextPowerTwoHeight = (uint)1 << (int)logHeight;

            if (level < 3)
            {
                if (logHeight < logWidth)
                    offsetY = (uint)16 >> (int)level;
                else
                    offsetX = (uint)16 >> (int)level;

                offsetZ = 0;
            }
            else
            {
                uint offset;

                if (logWidth > logHeight)
                {
                    offset = (uint)(nextPowerTwoWidth >> (int)(level - 2));
                    offsetX = offset;
                    offsetY = 0;
                }
                else
                {
                    offset = (uint)(nextPowerTwoHeight >> (int)(level - 2));
                    offsetY = offset;
                    offsetX = 0;
                }

                if (offset >= 4)
                    offsetZ = 0;
                else
                {
                    uint depthOffset = logDepth - level;
                    if (depthOffset <= 1)
                        depthOffset = 1;
                    offsetZ = 4 * depthOffset;
                }
            }

            uint xPixelOffset = offsetX;
            uint yPixelOffest = offsetY;

            offsetX /= blockWidth;
            offsetY /= blockHeight;

            return size * offsetZ + slicePitch * yPixelOffest + (xPixelOffset * bitsPerPixel * blockWidth >> 3);
        }

        /// <summary>
        /// This takes the current width and height and checks wether  the minimal dimension with border is &lt;= 16.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hasBorder"></param>
        /// <returns>0 if min dimension less or equal to 16, else returns the mipmap level at which the min width would be less or equal to 16</returns>
        private static uint GetMipLevelRequiresOffset(uint width, uint height, uint hasBorder)
        {
            uint logWidth = hasBorder + Log2Ceiling((int)(width - 2 * hasBorder - 1));
            uint logHeight = hasBorder + Log2Ceiling((int)(height - 2 * hasBorder - 1));
            uint minLogDim = logWidth;

            if (logWidth >= logHeight)
                minLogDim = logHeight;

            int result = (int)minLogDim - 4;

            if (result <= 0)
                result = 0;

            return (uint)result;
        }

        public static uint NextMultipleOf(uint value, uint multiple)
        {
            return ~(multiple - 1) & (value + multiple - 1);
        }

        public static uint AlignTextureDimensions(ref uint width, ref uint height, ref uint depth, uint bitsPerPixel, TextureFormat format, uint textueType, bool isTiled)
        {
            uint tileWidth = 32;
            uint tileHeight = textueType != 0 ? 32u : 1u;
            uint tileDepth = textueType != 2 ? 1u : 4u;

            uint blockWidth, blockHeight;
            XGGetBlockDimensions(format, out blockWidth, out blockHeight);

            if (!isTiled)
            {
                uint texelPitch = blockHeight * blockWidth * bitsPerPixel >> 3;
                if (texelPitch > 0)
                {
                    if (tileWidth <= 0x100 / texelPitch)
                        tileWidth = 0x100 / texelPitch;
                }
            }
            tileWidth *= blockWidth;
            tileHeight *= blockHeight;
            width = NextMultipleOf(width, tileWidth);
            height = NextMultipleOf(height, tileHeight);
            depth = NextMultipleOf(depth, tileDepth);

            uint sizeBytes = height * (bitsPerPixel * width >> 3);
            if (textueType == 2) // volume
                sizeBytes = NextMultipleOf(depth * sizeBytes, 4096);
            else
                sizeBytes = NextMultipleOf(sizeBytes, 4096);

            return sizeBytes;
        }

        public static uint GetMipTailLevelOffsetCoords(uint width, uint height, uint depth, uint level, TextureFormat format, bool isTiled, bool hasBorder, XGPOINT point)
        {
            uint border = (uint)(hasBorder ? 1 : 0);
            uint mipLevelRequiresOffset = GetMipLevelRequiresOffset(width, height, border);

            if (level >= mipLevelRequiresOffset) // happens when the requested level bitmap dimensions are <= 16
            {
                uint logWidth = border + Log2Ceiling((int)(width - 2 * border - 1));
                uint logHeight = border + Log2Ceiling((int)(height - 2 * border - 1));
                uint logDepth = 0;
                if (depth > 1)
                    logDepth = border + Log2Ceiling((int)(depth - 2 * border - 1));

                width = (uint)1 << (int)(logWidth - mipLevelRequiresOffset);
                height = (uint)1 << (int)(logHeight - mipLevelRequiresOffset);
                if (logDepth - mipLevelRequiresOffset <= 0)
                    depth = 1;
                else
                    depth = (uint)1 << (int)(logDepth - mipLevelRequiresOffset);

                uint bitsPerPixel = XGBitsPerPixelFromGpuFormat(format);
                uint tilePixelWidth = width;
                uint tilePixelHeight = height;
                uint tilePixelDepth = depth;
                uint xOffset = 0;
                uint yOffset = 0;
                uint zOffset = 0;

                AlignTextureDimensions(ref tilePixelWidth, ref tilePixelHeight, ref tilePixelDepth, bitsPerPixel, format, 1, isTiled);
                // previous size maybe?
                uint size = (tilePixelHeight * tilePixelWidth * bitsPerPixel) >> 3;
                if (depth <= 1)
                    size = AlignToPage(size);   // probably not required on PC

                uint result = D3DGetMipTailLevelOffsetCoords(level - mipLevelRequiresOffset, width, height, depth, bitsPerPixel * width >> 3, size, format, ref xOffset, ref yOffset, ref zOffset);
                point.X = (int)xOffset;
                point.Y = (int)yOffset;
                point.Z = (int)zOffset;
                return result;
            }
            else
            {
                point.X = 0;
                point.Y = 0;
                point.Z = 0;
                return 0;
            }

        }




        public static byte[] XGUntileTextureLevel(uint width, uint height, uint level, TextureFormat format, XGTILE flags, uint rowPitch, XGPOINT point, byte[] source, D3DBOX box)
        {
            uint width_as_blocks;
            uint height_as_blocks;
            uint texelPitch;

            uint blockWidth = 0;
            uint blockHeight = 0;


            XGGetBlockDimensions(format, out blockWidth, out blockHeight);
            int blockLogWidth = (int)Log2Floor((int)blockWidth);
            int blockLogHeight = (int)Log2Floor((int)blockHeight);
            var bitsPerPixel = XGBitsPerPixelFromGpuFormat(format);
            texelPitch = (bitsPerPixel << (blockLogWidth + blockLogHeight)) >> 3; // also bytes per block


            int borderSize = flags.HasFlag(XGTILE.XGTILE_BORDER) ? 2 : 0;
            int hasBorder = flags.HasFlag(XGTILE.XGTILE_BORDER) ? 1 : 0;

            if (level > 0)
            {
                int nextPowerOfTwoWidth = 1 << (hasBorder - (int)Log2Ceiling((int)(width - borderSize - 1))) >> (int)level;
                int nextPowerOfTwoHeight = 1 << (hasBorder - (int)Log2Ceiling((int)(height - borderSize - 1))) >> (int)level;

                if (nextPowerOfTwoWidth <= 1)
                    nextPowerOfTwoWidth = 1;
                if (nextPowerOfTwoHeight <= 1)
                    nextPowerOfTwoHeight = 1;

                width_as_blocks = (uint)(nextPowerOfTwoWidth + blockWidth - 1) >> blockLogWidth;
                height_as_blocks = (uint)(nextPowerOfTwoHeight + blockHeight - 1) >> blockLogHeight;
            }
            else
            {
                width_as_blocks = (width + blockWidth - 1) >> blockLogWidth;
                height_as_blocks = (height + blockHeight - 1) >> blockLogHeight;
            }

            // update point to be in terms of the block width and height
            if (point != null)
            {
                point.X >>= blockLogWidth;
                point.Y >>= blockLogWidth;
            }
            else
            {
                point = new XGPOINT();
                point.X = 0;
                point.Y = 0;
            }

            // update box bounds to be in terms of the block width and height
            if (box != null)
            {
                box.Left >>= blockLogWidth;
                box.Right = (box.Right + blockWidth - 1) >> blockLogWidth;
                box.Top >>= blockLogHeight;
                box.Bottom = (box.Bottom + blockHeight - 1) >> blockLogHeight;
            }
            else
            {
                box = new D3DBOX();
                box.Left = 0;
                box.Top = 0;

                var tempWidth = (width - borderSize) >> (int)level;
                if (tempWidth <= 1)
                    tempWidth = 1;
                box.Right = (uint)(tempWidth + blockWidth - 1) >> blockLogWidth;

                var tempHeight = (height - borderSize) >> (int)level;
                if (tempHeight <= 1)
                    tempHeight = 1;
                box.Bottom = (uint)(tempHeight + blockHeight - 1) >> blockLogHeight;


            }

            if (!flags.HasFlag(XGTILE.XGTILE_NONPACKED))
            {
                XGPOINT offset = new XGPOINT();
                // need to understand the return value and modify the byte[]
                var offsetInByteArray = GetMipTailLevelOffsetCoords(width, height, 1, level, format, true, flags.HasFlag(XGTILE.XGTILE_BORDER), offset);

                box.Top += (uint)offset.Y;
                box.Bottom += (uint)offset.Y;
                box.Left += (uint)offset.X;
                box.Right += (uint)offset.X;
            }

            return UntileSurface(width_as_blocks, height_as_blocks, rowPitch, point, source, texelPitch, box);
        }

        /// <summary>
        /// Untile surface. The input dimensions must be in terms of blocks.
        /// </summary>
        /// <param name="width">Width of the surface in blocks</param>
        /// <param name="height">Height of the surface in blocks</param>
        /// <param name="rowPitch">Size in bytes of a row of pixels in the destination image</param>
        /// <param name="point">Offset in the surface</param>
        /// <param name="source">Source data</param>
        /// <param name="texelPitch">Size in bytes of a block</param>
        /// <param name="rect">Image rectangle to untile</param>
        /// <returns></returns>
        private static byte[] UntileSurface(uint width, uint height, uint rowPitch, XGPOINT point, byte[] source, uint texelPitch, D3DBOX rect)
        {
            uint nBlocksWidth = rect.Right - rect.Left;
            uint nBlocksHeight = rect.Bottom - rect.Top;

            uint alignedWidth = (width + 31) & ~31u;
            uint alignedHeight = (height + 31) & ~31u;

            uint totalSize = AlignToPage(alignedWidth * alignedHeight); // may not be necessary on PC

            byte[] result = new byte[totalSize];

            uint v12 = 16 / texelPitch;
            uint logBpp = XGLog2LE16(texelPitch); // log bytes per pixel
            uint v14 = (~(v12 - 1u) & (rect.Left + v12)) - rect.Left; //  v12 - (rect.Left) % v12
            uint v42 = (~(v12 - 1u) & (rect.Left + nBlocksWidth)) - rect.Left; // nBlocksWidth - (rect.Left + nBlocksWidth) % v12


            //int x = XGAddress2DTiledX(offset, xChunks, texPitch);
            //int y = XGAddress2DTiledY(offset, xChunks, texPitch);
            //int sourceIndex = ((i * xChunks) * texPitch) + (j * texPitch);
            //int destinationIndex = ((y * xChunks) * texPitch) + (x * texPitch);

            for (uint yBlockIndex = 0; yBlockIndex < nBlocksHeight; yBlockIndex++)
            {
                uint v38 = alignedWidth >> 5;
                uint _y = yBlockIndex + rect.Top;
                uint v47 = v38 * (_y >> 5);
                uint v44 = (_y >> 4) & 1;
                uint yBlockOffset = (uint)point.Y;
                uint v17 = (_y >> 3) & 1;
                uint v46 = 16 * (_y & 1);
                uint v45 = 2 * v17;
                uint v19 = rect.Left;
                uint v18 = 4 * (_y & 6);
                uint v52 = v17 << (int)(logBpp + 6);
                uint heightOffset = rowPitch * (yBlockIndex + yBlockOffset);

                {
                    uint v30 = rect.Left;
                    uint v31 = (v44 + 2 * ((v45 + (byte)(v19 >> 3)) & 3));
                    uint micro = (v18 + (v30 & 7)) << (int)(logBpp + 6);
                    uint v32 = v46 + v52 + ((micro >> 6) & 0xF) + 2 * (((micro >> 6) & ~0xFu) + (((v47 + (v19 >> 5)) << (int)(logBpp + 6)) & 0x1FFFFFFF));
                    uint v28 = ((v32 >> 6) & 7) + 8 * (v31 & 1);

                    var v37a = v14;
                    if (v37a > nBlocksWidth)
                        v37a = nBlocksWidth;

                    uint sourceOffset = 8 * ((v32 & ~0x1FFu) + 4 * ((v31 & ~1u) + 8 * v28)) + (v32 & 0x3F);
                    uint destinationOffset = heightOffset + ((uint)point.X << (int)logBpp);
                    uint blockSize = v37a;

                    Array.Copy(source, sourceOffset, result, destinationOffset, blockSize);
                }

                uint x = v14;
                uint v48 = v14;

                while (x < v42)
                {
                    uint v30 = x + rect.Left;
                    uint v31 = v44 + 2 * ((v45 + (byte)(v30 >> 3)) & 3);
                    uint v25 = (v18 + (v30 & 7)) << (int)(logBpp + 6);
                    uint v32 = v46 + v52 + ((v25 >> 6) & 0xF) + 2 * (((v25 >> 6) & ~0xFu) + (((v47 + (v30 >> 5)) << (int)(logBpp + 6)) & 0x1FFFFFFF));
                    uint v28 = ((v32 >> 6) & 7) + 8 * (v31 & 1);

                    uint sourceOffset = 8 * ((v32 & ~0x1FFu) + 4 * ((v31 & ~1u) + 8 * v28)) + (v32 & 0x3F);
                    uint destinationOffset = heightOffset + ((v48 + (uint)point.X) << (int)logBpp);
                    uint blockSize = v12 << (int)logBpp;

                    Array.Copy(source, sourceOffset, result, destinationOffset, blockSize);

                    x += v12;
                }

                if (x < nBlocksWidth)
                {
                    uint v30 = x + rect.Left;
                    uint v31 = v44 + 2 * ((v45 + (byte)(v30 >> 3)) & 3);
                    uint v25 = (v18 + (v30 & 7)) << (int)(logBpp + 6);
                    uint v32 = v46 + v52 + ((v25 >> 6) & 0xF) + 2 * (((v25 >> 6) & ~0xFu) + (((v47 + (v30 >> 5)) << (int)(logBpp + 6)) & 0x1FFFFFFF));
                    uint v28 = ((v32 >> 6) & 7) + 8 * (v31 & 1);

                    uint sourceOffset = 8 * ((v32 & ~0x1FFu) + 4 * ((v31 & ~1u) + 8 * v28)) + (v32 & 0x3F);
                    uint destinationOffset = heightOffset + ((v48 + (uint)point.X) << (int)logBpp);
                    uint blockSize = (nBlocksWidth - v48) << (int)logBpp;

                    Array.Copy(source, sourceOffset, result, destinationOffset, blockSize);
                }
            }

            return result;
        }

        private static uint AlignToPage(uint offset)
        {
            return (offset + 0xFFF) & 0xFFFFF000;
        }

        public static uint XGLog2LE16(uint texelPitch)
        {
            return (texelPitch >> 2) + ((texelPitch >> 1) >> (int)(texelPitch >> 2));
        }

        public static uint Log2Floor(int input)
        {
            uint result = 0;
            do
            {
                if (input < 0)
                    break;
                input *= 2;
                result++;
            }
            while (result < 32);
            return 31 - result;
        }

        public static uint Log2Ceiling(int input)
        {
            uint result = 0;
            do
            {
                if (input < 0)
                    break;
                input *= 2;
                result++;
            }
            while (result < 32);
            return 32 - result;
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
