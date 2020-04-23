using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using IO;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

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

        // Xbox 360 formats
        Format_A4R4G4B4,        // 0x1828014f
    }

    public enum TextureType : int
    {
        Type_2D = 2,
        Type_CubeMap = 3,
        Type_DepthMap = 4,
    }

    [Flags]
    public enum TextureFlags : byte
    {
        HasD3DClearColor = 4,       // Texture has a R32G32B32A32_FLOAT color used to clear the scene with before rendering
    }

    public struct rTextureHeader
    {
        public const int kSizeOf = 24;
        public const int kMagic = 0x00584554;
        public const int kVersion = 0x56;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public byte Version;
        /* 0x05 */ public TextureType TextureType;
        /* 0x06 */ public TextureFlags Flags;
        /* 0x07 */ public byte MipMapCount;
        /* 0x08 */ public int Width;
        /* 0x0C */ public int Height;
        /* 0x10 */ public int Depth;
        /* 0x14 */ public TextureFormat Format;
    }

    [GameResourceParser(ResourceType.rTexture)]
    public class rTexture : GameResource
    {
        // Image header data.
        public rTextureHeader header;

        // Background color used for loading screen images.
        public float[] BackgroundColor = new float[4];

        // Data stream that pins the managed byte[] and allows us to pass its address to the directx layer.
        public DataStream PixelDataStream { get; private set; }

        // Sub resource array, one for each each mip level for each face of the texture.
        public DataBox[] SubResources { get; private set; }

        public int Width { get { return this.header.Width; } }
        public int Height { get { return this.header.Height; } }
        public int Depth { get { return this.header.Depth; } }
        public int MipMapCount { get { return this.header.MipMapCount; } }
        public TextureType Type { get { return this.header.TextureType; } }
        public TextureFormat Format { get { return this.header.Format; } }
        public TextureFlags Flags { get { return this.header.Flags; } }
        public bool Swizzled { get; private set; }
        public bool XboxFormat { get; private set; }
        public int FaceCount { get; private set; }

        public rTexture(string fileName, ResourceType fileType, bool isBigEndian)
            : base(fileName, fileType, isBigEndian)
        {

        }

        public Bitmap GetBitmap(int lod)
        {
            byte[] pixelData;

            // Set the bytes-per-pixel and image format to default values.
            int bpp = 4;
            PixelFormat pixelFormat = PixelFormat.Format32bppArgb;

            // Calculate the width and height for this lod.
            int width = this.header.Width >> lod;
            int height = this.header.Height >> lod;

            // Make sure the width and height are valid.
            if (width == 0 || height == 0)
                return null;

            // Make sure the mip level is valid.
            if (lod >= this.header.MipMapCount)
                return null;

            // Check the bitmap type and handle accordingly.
            switch (this.header.TextureType)
            {
                case (TextureType)1:
                case TextureType.Type_2D:
                case TextureType.Type_DepthMap:
                    {
                        // Decode the selected mip level.
                        pixelData = DecodeMipMap(0, lod, out pixelFormat);
                        break;
                    }
                case TextureType.Type_CubeMap:
                    {
                        if (width != height)
                        {

                        }

                        // Allocate a new pixel buffer that can hold 12 faces.
                        pixelData = new byte[(width * height * 4) * 12];

                        // Loop for the number of faces in the cube map and decode each one at the current level.
                        for (int i = 0; i < 6; i++)
                        {
                            // Decode the data for the current face.
                            byte[] facePixelData = DecodeMipMap(i, lod, out pixelFormat);

                            // Setup the sizes for each pixel buffer so we can blit the decoded face into the full image.
                            Size imageSize = new Size(width * 4, width * 3);
                            Size faceSize = new Size(width, width);
                            Point blitPoint = new Point(0, 0);

                            // Blit the current face into the full image in the correct position.
                            switch (i)
                            {
                                case 0: blitPoint = new Point(0, width); break;             // Left
                                case 1: blitPoint = new Point(width * 2, width); break;     // Right
                                case 2: blitPoint = new Point(width * 3, 0); break;         // Top
                                case 3: blitPoint = new Point(width * 3, width * 2); break; // Bottom
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

        private byte[] DecodeMipMap(int face, int lod, out PixelFormat pixelFormat)
        {
            // Set the default pixel format.
            pixelFormat = PixelFormat.Format32bppArgb;

            // Calculate the new width and height for the selected mip level.
            int width = this.header.Width >> lod;
            int height = this.header.Height >> lod;

            // Make sure the width and height are valid.
            if (width == 0 || height == 0)
                return new byte[0];

            // Get the sub resource description for this mip map for this face.
            DataBox subResource = this.SubResources[(face * this.header.MipMapCount) + lod];

            // Allocate the pixel buffer.
            byte[] pixelData = new byte[subResource.SlicePitch];

            // Seek to the pixel buffer in the data stream and read in the pixel buffer.
            this.PixelDataStream.Seek(subResource.DataPointer.ToInt64() - this.PixelDataStream.DataPointer.ToInt64(), SeekOrigin.Begin);
            this.PixelDataStream.Read(pixelData, 0, subResource.SlicePitch);

            // Check the bitmap format and decode accordingly.
            switch (this.header.Format)
            {
                case TextureFormat.Format_DXT1:
                    {
                        // DXT1:
                        pixelData = DXTDecoder.DecodeDXT1Texture(width, height, pixelData, this.IsBigEndian);
                        break;
                    }
                case TextureFormat.Format_DXT2:
                    {
                        // DXT2:
                        pixelData = DXTDecoder.DecodeDXT23Texture(width, height, pixelData, this.IsBigEndian);
                        break;
                    }
                case TextureFormat.Format_DXT5:
                    {
                        // DXT5:
                        pixelData = DXTDecoder.DecodeDXT45Texture(width, height, pixelData, this.IsBigEndian);
                        break;
                    }
                case TextureFormat.Format_R8G8_SNORM:
                    {
                        // R8G8_SNORM: 16bpp R8G8 normal map.
                        pixelData = DXTDecoder.DecodeR8G8(width, height, pixelData, this.IsBigEndian);
                        pixelFormat = PixelFormat.Format32bppRgb;
                        break;
                    }
                case TextureFormat.Format_B8G8R8A8_UNORM:
                    {
                        // B8G8R8A8_UNORM: 32bpp normal map
                        pixelFormat = PixelFormat.Format32bppRgb;
                        break;
                    }
                default:
                    {
                        // Unsupported bitmap format.
                        return null;
                    }
            }

            // Return the decoded pixel buffer.
            return pixelData;
        }

        public static rTexture FromGameResource(byte[] buffer, string fileName, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough to contain the texture header.
            if (buffer.Length < rTextureHeader.kSizeOf)
                return null;

            // Create a new texture object to populate with data.
            rTexture texture = new rTexture(fileName, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Parse the header.
            texture.header = new rTextureHeader();
            texture.header.Magic = reader.ReadInt32();
            texture.header.Version = reader.ReadByte();
            texture.header.TextureType = (TextureType)reader.ReadByte();
            texture.header.Flags = (TextureFlags)reader.ReadByte();
            texture.header.MipMapCount = reader.ReadByte();
            texture.header.Width = reader.ReadInt32();
            texture.header.Height = reader.ReadInt32();
            texture.header.Depth = reader.ReadInt32();
            int fourcc = reader.ReadInt32();
            texture.header.Format = TextureFormatFromFourCC(fourcc);
            texture.Swizzled = IsXboxFormat(fourcc);
            texture.XboxFormat = IsXboxFormat(fourcc);

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
            if (texture.header.Flags.HasFlag(TextureFlags.HasD3DClearColor) == true)
            {
                // Read the RGBA background color.
                texture.BackgroundColor[0] = reader.ReadSingle();
                texture.BackgroundColor[1] = reader.ReadSingle();
                texture.BackgroundColor[2] = reader.ReadSingle();
                texture.BackgroundColor[3] = reader.ReadSingle();
            }

            // Check for some unknown blob.
            if (texture.header.TextureType == TextureType.Type_CubeMap)
            {
                // Read 108 bytes.
                // These could be vertex coordinates for 9 vertices that make up the box the cubemap gets rendered on?
                reader.ReadBytes(108);
            }

            // Read all of the pixel data now and pin it so we can build the sub resources array.
            byte[] pixelData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            texture.PixelDataStream = DataStream.Create(pixelData, true, false);

            // Make sure we are at the start of the pixel data stream.
            texture.PixelDataStream.Seek(0, SeekOrigin.Begin);

            // TODO: Properly handle tiling on xbox 360 textures.
            //if (texture.Swizzled == true)
            //{
            //    //pixelData = Swizzle.ConvertToLinearTexture(pixelData, texture.header.Width, texture.header.Height, texture.header.Format);

            //    int rowPitch = ((texture.header.Width + 3) / 4);// * RowPitchFromTextureFormat(texture.header.Format);
            //    pixelData = Swizzle.XGUntileTextureLevel((uint)texture.header.Width, (uint)texture.header.Height, 0, texture.header.Format,
            //        Swizzle.XGTILE.XGTILE_BORDER, (uint)rowPitch, null, pixelData, null);
            //}

            // If the texture type is a raw DDS file read the DDS header now.
            if (texture.header.TextureType == TextureType.Type_DepthMap)
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
                reader.BaseStream.Position += sizeof(int); // BUG: We should use the header size to seek to pixel data

                // Check the header magic and structure sizes for sanity.
                if (ddsHeader.dwMagic != DDS_HEADER.kMagic || ddsHeader.dwSize != DDS_HEADER.kSizeOf ||
                    ddsHeader.ddspf.dwSize != DDS_PIXELFORMAT.kSizeOf)
                {
                    // DDS header is invalid.
                    return null;
                }

                // TODO: Save this info off
            }

            // Set the number of faces this texture has based on the texture type.
            if (texture.header.TextureType == TextureType.Type_CubeMap)
            {
                // Cubemap has 6 faces each with n mip map levels.
                texture.FaceCount = 6;
            }
            else
            {
                // All other texture types have 1 face with n mip maps.
                texture.FaceCount = 1;
            }

            // Allocate the sub resources array and setup for every face the texture has.
            texture.SubResources = new DataBox[texture.FaceCount * texture.header.MipMapCount];
            for (int i = 0; i < texture.FaceCount; i++)
            {
                // Loop for the number of mip maps and read each one.
                for (int x = 0; x < texture.header.MipMapCount; x++)
                {
                    int bytesPerRow = 0;
                    int numberOfBlocks = 0;

                    // Calculate the pitch values for the current mip level.
                    int mipHeight = texture.header.TextureType == TextureType.Type_CubeMap ? texture.header.Width : texture.header.Height;
                    CalculateMipMapPitch(texture.header.Width, mipHeight, x, texture.header.Format, out bytesPerRow, out numberOfBlocks);

                    // Setup the resource description for this mip map.
                    texture.SubResources[(i * texture.header.MipMapCount) + x] = new DataBox(texture.PixelDataStream.PositionPointer, bytesPerRow, bytesPerRow * numberOfBlocks);
                    texture.PixelDataStream.Seek(bytesPerRow * numberOfBlocks, SeekOrigin.Current);
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
                case 0x1a200152:
                case 0x31545844: return TextureFormat.Format_DXT1;
                case 0x32545844: return TextureFormat.Format_DXT2;
                case 0x1A200154:
                //case 0x1a20017b: DXT5A
                case 0x35545844: return TextureFormat.Format_DXT5;
                case 60: return TextureFormat.Format_R8G8_SNORM;
                case 0x18280186:
                case 21: return TextureFormat.Format_B8G8R8A8_UNORM;

                case 0x1828014f: return TextureFormat.Format_A4R4G4B4;
                default: return TextureFormat.Format_Unsupported;
            }
        }

        public static bool IsXboxFormat(int fourcc)
        {
            switch (fourcc)
            {
                case 0x1a200152:
                case 0x1A200154:
                case 0x18280186:
                case 0x1828014f:
                    return true;
            }

            // Format is not xbox.
            return false;
        }

        public static bool IsFormatSwizzled(int fourcc)
        {
            return IsXboxFormat(fourcc);
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

        public static int RowPitchFromTextureFormat(TextureFormat format)
        {
            // Check the texture format and handle accordingly.
            switch (format)
            {
                case TextureFormat.Format_DXT1: return 8;
                case TextureFormat.Format_DXT2:
                case TextureFormat.Format_DXT5: return 16;
                case TextureFormat.Format_R8G8_SNORM: return 2;
                case TextureFormat.Format_B8G8R8A8_UNORM: return 4;
                case TextureFormat.Format_A4R4G4B4: return 2;
                default: return 0;
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

        #region IRenderable

        public override bool InitializeGraphics(IRenderManager manager, Device device)
        {
            return true;
        }

        //public override bool DrawFrame(IRenderManager manager, Device device)
        //{
        //    throw new NotImplementedException();
        //}

        //public override void CleanupGraphics(IRenderManager manager, Device device)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
