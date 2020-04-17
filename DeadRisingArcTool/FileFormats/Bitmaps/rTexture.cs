﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    public enum TextureFormat : int
    {
        Format_Unsupported,
        Format_DXT1,            // DXGI_FORMAT: 71
        Format_DXT2,            // 74
        Format_DXT5,            // 77
        Format_R8G8_SNORM,      // 51
        Format_B8G8R8A8_UNORM,  // 87
    }

    public enum TextureType : int
    {
        Type_2D = 2,
        Type_CubeMap = 3,
        Type_DepthMap = 4,
    }

    public struct rTextureHeader
    {
        public const int kSizeOf = 24;
        public const int kMagic = 0x00584554;
        public const int kVersion = 0x56;

        public int Magic;
        public byte Version;
        public TextureType TextureType;
        public byte Flags;
        public byte MipMapCount;
        public int Width;
        public int Height;
        public int Depth;
        public TextureFormat Format;
    }

    public class rTexture
    {
        // Image header data.
        public rTextureHeader header;

        // Background color used for loading screen images.
        public float[] BackgroundColor = new float[4];

        // Array of pixel buffers for each mip map level.
        public byte[][][] mipMapPixelBuffers;

        public Bitmap GetBitmap(int lod)
        {
            int width, height;
            byte[] pixelData;

            // Set the bytes-per-pixel and image format to default values.
            int bpp = 4;
            PixelFormat pixelFormat = PixelFormat.Format32bppArgb;

            // Make sure the mip level is valid.
            if (lod >= this.header.MipMapCount)
                return null;

            // Check the bitmap type and handle accordingly.
            switch (this.header.TextureType)
            {
                case TextureType.Type_2D:
                case TextureType.Type_DepthMap:
                    {
                        // Calculate the new width and height for the selected mip level.
                        width = this.header.Width >> lod;
                        height = this.header.Height >> lod;

                        // Check the bitmap format and decode accordingly.
                        switch (this.header.Format)
                        {
                            case TextureFormat.Format_DXT1:
                                {
                                    // DXT1: 32bpp BGRA format.
                                    pixelData = DXTDecoder.DecodeDXT1(height, width, this.mipMapPixelBuffers[0][lod]);
                                    break;
                                }
                            case TextureFormat.Format_DXT2:
                                {
                                    // DXT2: 32bpp BGRA format.
                                    pixelData = DXTDecoder.DecodeDXT23(height, width, this.mipMapPixelBuffers[0][lod]);
                                    break;
                                }
                            case TextureFormat.Format_DXT5:
                                {
                                    // DXT5: 32bpp BGRA format.
                                    pixelData = DXTDecoder.DecodeDXT45(height, width, this.mipMapPixelBuffers[0][lod]);
                                    break;
                                }
                            case TextureFormat.Format_R8G8_SNORM:
                                {
                                    // R8G8_SNORM: 16bpp R8G8 normal map.
                                    pixelData = DXTDecoder.DecodeR8G8(width, height, this.mipMapPixelBuffers[0][lod]);
                                    pixelFormat = PixelFormat.Format32bppRgb;
                                    break;
                                }
                            case TextureFormat.Format_B8G8R8A8_UNORM:
                                {
                                    // B8G8R8A8_UNORM: 32bpp normal map
                                    pixelData = this.mipMapPixelBuffers[0][lod];
                                    pixelFormat = PixelFormat.Format32bppRgb;
                                    break;
                                }
                            default:
                                {
                                    // Unsupported bitmap format.
                                    return null;
                                }
                        }
                        break;
                    }
                case TextureType.Type_CubeMap:
                    {
                        int bytesPerRow;
                        int numberOfBlocks;

                        // Calculate the size of a single face at the current lod.
                        CalculateMipMapPitch(this.header.Width, this.header.Width, lod, this.header.Format, out bytesPerRow, out numberOfBlocks);

                        // Calculate the new width and height for the selected mip level.
                        width = this.header.Width >> lod;
                        height = this.header.Height >> lod;

                        if (width != height)
                        {

                        }

                        // Allocate a new pixel buffer that can hold 12 faces.
                        pixelData = new byte[(width * height * 4) * 12];

                        // Loop for the number of faces in the cube map and decode each one at the current level.
                        for (int i = 0; i < 6; i++)
                        {
                            byte[] facePixelData;

                            // Check the bitmap format and handle accordingly.
                            switch (this.header.Format)
                            {
                                case TextureFormat.Format_DXT1:
                                    {
                                        // DXT1: 32bpp BGRA format.
                                        facePixelData = DXTDecoder.DecodeDXT1(height, width, this.mipMapPixelBuffers[i][lod]);
                                        break;
                                    }
                                case TextureFormat.Format_DXT2:
                                    {
                                        // DXT2: 32bpp BGRA format.
                                        facePixelData = DXTDecoder.DecodeDXT23(height, width, this.mipMapPixelBuffers[i][lod]);
                                        break;
                                    }
                                case TextureFormat.Format_DXT5:
                                    {
                                        // DXT5: 32bpp BGRA format.
                                        facePixelData = DXTDecoder.DecodeDXT45(height, width, this.mipMapPixelBuffers[i][lod]);
                                        break;
                                    }
                                case TextureFormat.Format_R8G8_SNORM:
                                    {
                                        // R8G8_SNORM: 16bpp R8G8 normal map.
                                        facePixelData = DXTDecoder.DecodeR8G8(width, height, this.mipMapPixelBuffers[i][lod]);
                                        pixelFormat = PixelFormat.Format32bppRgb;
                                        break;
                                    }
                                case TextureFormat.Format_B8G8R8A8_UNORM:
                                    {
                                        // B8G8R8A8_UNORM: 32bpp normal map
                                        facePixelData = this.mipMapPixelBuffers[i][lod];
                                        pixelFormat = PixelFormat.Format32bppRgb;
                                        break;
                                    }
                                default:
                                    {
                                        // Unsupported bitmap format.
                                        return null;
                                    }
                            }

                            // Setup the sizes for each pixel buffer so we can blit the decoded face into the full image.
                            Size imageSize = new Size(width * 4, width * 3);
                            Size faceSize = new Size(width, width);
                            Point blitPoint = new Point(0, 0);

                            // Blit the current face into the full image in the correct position.
                            switch (i)
                            {
                                case 0: blitPoint = new Point(0, width); break;             // Left
                                case 1: blitPoint = new Point(width * 2, width); break;     // Right
                                case 2: blitPoint = new Point(width * 3, 0); break;             // Top
                                case 3: blitPoint = new Point(width * 3, width * 2); break;     // Bottom
                                case 4: blitPoint = new Point(width * 3, width); break;     // Back
                                case 5: blitPoint = new Point(width, width); break;         // Front
                            }

                            // Blit the face into the final image.
                            Blit(pixelData, imageSize, facePixelData, faceSize, blitPoint);
                        }

                        // Adjust the width and height for the final image.
                        width *= 4;
                        height *= 3;
                        break;
                    }
                default:
                    {
                        // Unsupported bitmap type.
                        return null;
                    }
            }

            // Create a new bitmap with the correct dimensions and pixel format.
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);

            // Lock the bitmap buffer so we can copy in the decoded pixel data.
            BitmapData lockedData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);

            // Copy the decoded pixel data.
            Marshal.Copy(pixelData, 0, lockedData.Scan0, width * height * bpp);

            // Unlock the pixel data and return the bitmap object.
            bitmap.UnlockBits(lockedData);
            return bitmap;
        }

        public static rTexture FromBuffer(byte[] buffer)
        {
            // Make sure the buffer is large enough to contain the texture header.
            if (buffer.Length < rTextureHeader.kSizeOf)
                return null;

            // Create a new texture object to populate with data.
            rTexture texture = new rTexture();

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(ms);

            // Parse the header.
            texture.header = new rTextureHeader();
            texture.header.Magic = reader.ReadInt32();
            texture.header.Version = reader.ReadByte();
            texture.header.TextureType = (TextureType)reader.ReadByte();
            texture.header.Flags = reader.ReadByte();
            texture.header.MipMapCount = reader.ReadByte();
            texture.header.Width = reader.ReadInt32();
            texture.header.Height = reader.ReadInt32();
            texture.header.Depth = reader.ReadInt32();
            texture.header.Format = TextureFormatFromFourCC(reader.ReadInt32());

            // Verify the magic is correct.
            if (texture.header.Magic != rTextureHeader.kMagic)
            {
                // Header has invalid magic.
                return null;
            }

            // Check the version is supported.
            if (texture.header.Version != rTextureHeader.kVersion)
            {
                // Texture is an unsupported version.
                return null;
            }

            // Make sure the texture format is supported.
            if (texture.header.Format == TextureFormat.Format_Unsupported)
            {
                // Texture is unsupported format.
                return null;
            }

            // Check if we need to read the background color.
            if ((texture.header.Flags & 4) != 0)
            {
                // Read the RGBA background color.
                texture.BackgroundColor[0] = reader.ReadSingle();
                texture.BackgroundColor[1] = reader.ReadSingle();
                texture.BackgroundColor[2] = reader.ReadSingle();
                texture.BackgroundColor[3] = reader.ReadSingle();
            }

            // Check for some unknown blob, UV data maybe?
            if (texture.header.TextureType == TextureType.Type_CubeMap)
            {
                // Read 108 bytes.
                reader.ReadBytes(108);
            }

            // Check the texture type and handle accordingly.
            switch (texture.header.TextureType)
            {
                case TextureType.Type_2D:
                    {
                        // Allocate the mip map pixel data buffer.
                        texture.mipMapPixelBuffers = new byte[1][][];
                        texture.mipMapPixelBuffers[0] = new byte[texture.header.MipMapCount][];

                        // Loop for each mip map and setup the pixel data buffers.
                        for (int i = 0; i < texture.header.MipMapCount; i++)
                        {
                            int bytesPerRow = 0;
                            int numberOfBlocks = 0;

                            // Calculate the pitch values for the current mip level.
                            CalculateMipMapPitch(texture.header.Width, texture.header.Height, i, texture.header.Format, out bytesPerRow, out numberOfBlocks);

                            // Save the pixel data for the current mip level.
                            texture.mipMapPixelBuffers[0][i] = reader.ReadBytes(bytesPerRow * numberOfBlocks);
                        }
                        break;
                    }
                case TextureType.Type_CubeMap:
                    {
                        // Allocate the pixel buffers for each face.
                        texture.mipMapPixelBuffers = new byte[6][][];

                        // Loop for the number of sides in the cubemap.
                        for (int i = 0; i < 6; i++)
                        {
                            // Allocate pixel buffers for each mip map of this face.
                            texture.mipMapPixelBuffers[i] = new byte[texture.header.MipMapCount][];

                            // Loop for each mip map and read each level of pixel data.
                            for (int x = 0; x < texture.header.MipMapCount; x++)
                            {
                                int bytesPerRow = 0;
                                int numberOfBlocks = 0;

                                // Calculate the pitch values for the current mip level.
                                CalculateMipMapPitch(texture.header.Width, texture.header.Width, x, texture.header.Format, out bytesPerRow, out numberOfBlocks);

                                // Read the pixel data for the current face's mip level.
                                texture.mipMapPixelBuffers[i][x] = reader.ReadBytes(bytesPerRow * numberOfBlocks);
                            }
                        }
                        break;
                    }
                case TextureType.Type_DepthMap:
                    {
                        // Read the DDS image header.
                        DDS_HEADER ddsHeader = new DDS_HEADER();
                        ddsHeader.dwMagic = reader.ReadInt32();
                        ddsHeader.dwSize = reader.ReadInt32();
                        ddsHeader.dwFlags = reader.ReadInt32();
                        ddsHeader.dwHeight = reader.ReadInt32();
                        ddsHeader.dwWidth = reader.ReadInt32();
                        ddsHeader.dwPitchOrLinearSize = reader.ReadInt32();
                        ddsHeader.dwDepth = reader.ReadInt32();
                        ddsHeader.dwMipMapCount = reader.ReadInt32();
                        reader.BaseStream.Position += sizeof(int) * 11;
                        ddsHeader.ddspf = new DDS_PIXELFORMAT();
                        ddsHeader.ddspf.dwSize = reader.ReadInt32();
                        ddsHeader.ddspf.dwFlags = reader.ReadInt32();
                        ddsHeader.ddspf.dwFourCC = reader.ReadInt32();
                        ddsHeader.ddspf.dwRGBBitCount = reader.ReadInt32();
                        ddsHeader.ddspf.dwRBitMask = reader.ReadInt32();
                        ddsHeader.ddspf.dwGBitMask = reader.ReadInt32();
                        ddsHeader.ddspf.dwBBitMask = reader.ReadInt32();
                        ddsHeader.ddspf.dwABitMask = reader.ReadInt32();
                        ddsHeader.dwCaps = reader.ReadInt32();
                        ddsHeader.dwCaps2 = reader.ReadInt32();
                        ddsHeader.dwCaps3 = reader.ReadInt32();
                        ddsHeader.dwCaps4 = reader.ReadInt32();
                        reader.BaseStream.Position += sizeof(int);

                        // Check the header magic and structure sizes for sanity.
                        if (ddsHeader.dwMagic != DDS_HEADER.kMagic || ddsHeader.dwSize != DDS_HEADER.kSizeOf || 
                            ddsHeader.ddspf.dwSize != DDS_PIXELFORMAT.kSizeOf)
                        {
                            // DDS header is invalid.
                            return null;
                        }

                        // Allocate the mip map pixel data buffer.
                        texture.mipMapPixelBuffers = new byte[1][][];
                        texture.mipMapPixelBuffers[0] = new byte[texture.header.MipMapCount][];

                        // Loop for each mip map and setup the pixel data buffers.
                        for (int i = 0; i < texture.header.MipMapCount; i++)
                        {
                            int bytesPerRow = 0;
                            int numberOfBlocks = 0;

                            // Calculate the pitch values for the current mip level.
                            CalculateMipMapPitch(texture.header.Width, texture.header.Height, i, texture.header.Format, out bytesPerRow, out numberOfBlocks);

                            // Save the pixel data for the current mip level.
                            texture.mipMapPixelBuffers[0][i] = reader.ReadBytes(bytesPerRow * numberOfBlocks);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            // Close the binary reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the texture object.
            return texture;
        }

        #region Utilities

        public static TextureFormat TextureFormatFromFourCC(int fourcc)
        {
            switch (fourcc)
            {
                case 0x31545844: return TextureFormat.Format_DXT1;
                case 0x32545844: return TextureFormat.Format_DXT2;
                case 0x35545844: return TextureFormat.Format_DXT5;
                case 60: return TextureFormat.Format_R8G8_SNORM;
                case 21: return TextureFormat.Format_B8G8R8A8_UNORM;
                default: return TextureFormat.Format_Unsupported;
            }
        }

        public static SharpDX.DXGI.Format DXGIFromTextureFormat(TextureFormat format)
        {
            // Check the texture format and handle accordingly.
            switch (format)
            {
                case TextureFormat.Format_DXT1: return SharpDX.DXGI.Format.BC1_UNorm;
                case TextureFormat.Format_DXT2: return SharpDX.DXGI.Format.BC2_UNorm;
                case TextureFormat.Format_DXT5: return SharpDX.DXGI.Format.BC3_UNorm;
                case TextureFormat.Format_R8G8_SNORM: return SharpDX.DXGI.Format.R8G8_SNorm;
                case TextureFormat.Format_B8G8R8A8_UNORM: return SharpDX.DXGI.Format.B8G8R8A8_UNorm;

                case TextureFormat.Format_Unsupported:
                default: return SharpDX.DXGI.Format.Unknown;
            }
        }

        public static void CalculateMipMapPitch(int width, int height, int lod, TextureFormat format, out int bytesPerRow, out int numberOfBlocks)
        {
            // Calculate the new width and height for this mip level.
            int newWidth = (width >> lod) > 1 ? (width >> lod) : 1;
            int newHeight = (height >> lod) > 1 ? (height >> lod) : 1;

            // Calculate the pitch values based on the texture format.
            switch (format)
            {
                case TextureFormat.Format_DXT1:
                    {
                        // Block size = 8 bytes per 4x4 block
                        bytesPerRow = (newWidth + 3) / 4 > 1 ? (newWidth + 3) / 4 : 1;
                        bytesPerRow *= 8;

                        // Number of blocks.
                        numberOfBlocks = (newHeight + 3) / 4 > 1 ? (newHeight + 3) / 4 : 1;
                        break;
                    }
                case TextureFormat.Format_DXT2:
                case TextureFormat.Format_DXT5:
                    {
                        // Block size = 16 bytes per 4x4 block
                        bytesPerRow = (newWidth + 3) / 4 > 1 ? (newWidth + 3) / 4 : 1;
                        bytesPerRow *= 16;

                        // Number of blocks.
                        numberOfBlocks = (newHeight + 3) / 4 > 1 ? (newHeight + 3) / 4 : 1;
                        break;
                    }
                default:
                    {
                        // Calculate the bits per pixel for the texture format.
                        switch (format)
                        {
                            case TextureFormat.Format_R8G8_SNORM: bytesPerRow = 16; break;
                            case TextureFormat.Format_B8G8R8A8_UNORM: bytesPerRow = 32; break;
                            default:
                                {
                                    // Unsupported bitmap format.
                                    bytesPerRow = 0;
                                    break;
                                }
                        }

                        // Calculate bytes per row.
                        bytesPerRow = ((bytesPerRow * newWidth) / 8) + 3;
                        bytesPerRow = unchecked(bytesPerRow & (int)0xFFFFFFFC);

                        // Number of blocks.
                        numberOfBlocks = newHeight;
                        break;
                    }
            }
        }

        public static void Blit(byte[] destBuffer, Size destSize, byte[] sourceBuffer, Size sourceSize, Point blitPoint)
        {
            // Loop for the height of the source image.
            for (int i = 0; i < sourceSize.Height; i++)
            {
                // Calculate the offset to blit into the destination buffer.
                int dstOffset = (blitPoint.Y * destSize.Width * 4) + (i * destSize.Width * 4) + (blitPoint.X * 4);

                // Copy the current line of the source image into the destination image.
                Array.Copy(sourceBuffer, i * sourceSize.Width * 4, destBuffer, dstOffset, sourceSize.Width * 4);
            }
        }

        #endregion
    }
}
