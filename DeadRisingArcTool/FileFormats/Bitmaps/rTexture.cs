using DeadRisingArcTool.FileFormats.Archive;
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

        // Unknown cubemap data, maybe vertices?
        private byte[] cubemapData;

        // Additional header for depth maps.
        private DDS_HEADER depthMapHeader;

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

        public rTexture(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
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

        public override byte[] ToBuffer()
        {
            // Create a new memory stream to back our file with.
            MemoryStream ms = new MemoryStream();
            EndianWriter writer = new EndianWriter(this.IsBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Write the header fields.
            writer.Write(rTextureHeader.kMagic);
            writer.Write((byte)rTextureHeader.kVersion);
            writer.Write((byte)this.header.TextureType);
            writer.Write((byte)this.header.Flags);
            writer.Write(this.header.MipMapCount);
            writer.Write(this.header.Width);
            writer.Write(this.header.Height);
            writer.Write(this.header.Depth);
            writer.Write(FourCCFromTextureFormat(this.header.Format, this.XboxFormat));

            // Check if we need to write the background color.
            if (this.header.Flags.HasFlag(TextureFlags.HasD3DClearColor) == true)
            {
                // Write the background color.
                writer.Write(this.BackgroundColor[0]);
                writer.Write(this.BackgroundColor[1]);
                writer.Write(this.BackgroundColor[2]);
                writer.Write(this.BackgroundColor[3]);
            }

            // If this is a cubemap write the cubemap data.
            if (this.header.TextureType == TextureType.Type_CubeMap)
            {
                // Write the cubemap data.
                writer.Write(this.cubemapData);
            }

            // If this is a depth map we need to write a DDS header for it.
            if (this.header.TextureType == TextureType.Type_DepthMap)
            {
                // Write the DDS header for the depth map.
                writer.Write(this.depthMapHeader.dwMagic);
                writer.Write(this.depthMapHeader.dwSize);
                writer.Write((int)this.depthMapHeader.dwFlags);
                writer.Write(this.depthMapHeader.dwHeight);
                writer.Write(this.depthMapHeader.dwWidth);
                writer.Write(this.depthMapHeader.dwPitchOrLinearSize);
                writer.Write(this.depthMapHeader.dwDepth);
                writer.Write(this.depthMapHeader.dwMipMapCount);
                writer.WritePadding(11 * 4);
                writer.Write(this.depthMapHeader.ddspf.dwSize);
                writer.Write((int)this.depthMapHeader.ddspf.dwFlags);
                writer.Write(this.depthMapHeader.ddspf.dwFourCC);
                writer.Write(this.depthMapHeader.ddspf.dwRGBBitCount);
                writer.Write(this.depthMapHeader.ddspf.dwRBitMask);
                writer.Write(this.depthMapHeader.ddspf.dwGBitMask);
                writer.Write(this.depthMapHeader.ddspf.dwBBitMask);
                writer.Write(this.depthMapHeader.ddspf.dwABitMask);
                writer.Write((int)this.depthMapHeader.dwCaps);
                writer.Write((int)this.depthMapHeader.dwCaps2);
                writer.Write(this.depthMapHeader.dwCaps3);
                writer.Write(this.depthMapHeader.dwCaps4);
                writer.WritePadding(4);
            }

            // Read the pixel buffer from the data stream.
            byte[] pixelData = new byte[this.PixelDataStream.Length];
            this.PixelDataStream.Seek(0, SeekOrigin.Begin);
            this.PixelDataStream.Read(pixelData, 0, pixelData.Length);

            // TODO: For xbox textures we need to tile the pixel buffer.

            // Write the pixel data.
            writer.Write(pixelData);

            // Close the binary writer and return the memory stream as a byte array.
            writer.Close();
            return ms.ToArray();
        }

        #region FromGameResource/DDSImage

        public static rTexture FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough to contain the texture header.
            if (buffer.Length < rTextureHeader.kSizeOf)
                return null;

            // Create a new texture object to populate with data.
            rTexture texture = new rTexture(fileName, datum, fileType, isBigEndian);

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
                texture.cubemapData = reader.ReadBytes(108);
            }

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
                texture.depthMapHeader = new DDS_HEADER();
                texture.depthMapHeader.dwMagic = reader.ReadInt32();
                texture.depthMapHeader.dwSize = reader.ReadInt32();
                texture.depthMapHeader.dwFlags = (DDSD_FLAGS)reader.ReadInt32();
                texture.depthMapHeader.dwHeight = reader.ReadInt32();
                texture.depthMapHeader.dwWidth = reader.ReadInt32();
                texture.depthMapHeader.dwPitchOrLinearSize = reader.ReadInt32();
                texture.depthMapHeader.dwDepth = reader.ReadInt32();
                texture.depthMapHeader.dwMipMapCount = reader.ReadInt32();
                reader.BaseStream.Position += sizeof(int) * 11;
                texture.depthMapHeader.ddspf = new DDS_PIXELFORMAT();
                texture.depthMapHeader.ddspf.dwSize = reader.ReadInt32();
                texture.depthMapHeader.ddspf.dwFlags = (DDPF)reader.ReadInt32();
                texture.depthMapHeader.ddspf.dwFourCC = reader.ReadInt32();
                texture.depthMapHeader.ddspf.dwRGBBitCount = reader.ReadInt32();
                texture.depthMapHeader.ddspf.dwRBitMask = reader.ReadUInt32();
                texture.depthMapHeader.ddspf.dwGBitMask = reader.ReadUInt32();
                texture.depthMapHeader.ddspf.dwBBitMask = reader.ReadUInt32();
                texture.depthMapHeader.ddspf.dwABitMask = reader.ReadUInt32();
                texture.depthMapHeader.dwCaps = (DDSCAPS)reader.ReadInt32();
                texture.depthMapHeader.dwCaps2 = (DDSCAPS2)reader.ReadInt32();
                texture.depthMapHeader.dwCaps3 = reader.ReadInt32();
                texture.depthMapHeader.dwCaps4 = reader.ReadInt32();
                reader.BaseStream.Position += sizeof(int); 
                // BUG: need to check for dx10 header.

                // Check the header magic and structure sizes for sanity.
                if (texture.depthMapHeader.dwMagic != DDS_HEADER.kMagic || texture.depthMapHeader.dwSize != DDS_HEADER.kSizeOf ||
                    texture.depthMapHeader.ddspf.dwSize != DDS_PIXELFORMAT.kSizeOf)
                {
                    // DDS header is invalid.
                    return null;
                }
            }

            // Read all of the pixel data now and pin it so we can build the sub resources array.
            byte[] pixelData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            texture.PixelDataStream = DataStream.Create(pixelData, true, false);

            // Make sure we are at the start of the pixel data stream.
            texture.PixelDataStream.Seek(0, SeekOrigin.Begin);

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

        public static rTexture FromDDSImage(DDSImage image, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Create a new texture we can populate with info.
            rTexture texture = new rTexture(fileName, datum, fileType, isBigEndian);

            // Fill out the header fields.
            texture.header.Magic = rTextureHeader.kMagic;
            texture.header.Version = rTextureHeader.kVersion;
            texture.header.Width = image.Width;
            texture.header.Height = image.Height;
            texture.header.MipMapCount = (byte)image.MipMapCount;
            texture.header.Depth = image.Depth;

            // Check various flags to determine what type of texture this is.
            if (image.Flags.HasFlag(DDSD_FLAGS.DDSD_DEPTH) == true)
            {
                // Set the depthmap texture type.
                texture.header.TextureType = TextureType.Type_DepthMap;

                // Copy the DDS header from the image.
                texture.depthMapHeader = image.header;
            }
            else if (image.Capabilities2.HasFlag(DDSCAPS2.DDSCAPS2_CUBEMAP_ALLFACES) == true)
                texture.header.TextureType = TextureType.Type_CubeMap;
            else
                texture.header.TextureType = TextureType.Type_2D;

            // Check the image's fourcc code to determine the texture format.
            if (image.Format == 0)
            {
                // Check the bit count and color masks to determine the texture format.
                if (image.header.ddspf.dwFlags.HasFlag(DDPF.DDPF_BUMPDUDV) == true || (image.BitCount == 16 && image.RBitMask == 0xFF && image.GBitMask == 0xFF00))
                    texture.header.Format = TextureFormat.Format_R8G8_SNORM;
                else if (image.BitCount == 32)
                    texture.header.Format = TextureFormat.Format_B8G8R8A8_UNORM;

                // TODO: Do we need to re-encode?
            }
            else
            {
                // Use the fourcc code to determine the texture format.
                texture.header.Format = DDSImage.TextureFormatFromFourCC(image.Format);
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

            // Calculate the total size of the pixel buffer.
            int pixelBufferSize = CalculatePixelBufferSize(texture.header.Width, texture.header.Height, texture.FaceCount, 
                texture.header.MipMapCount, texture.header.Format, texture.header.TextureType);

            // Check if we need to pad the pixel buffer from the DDS image.
            byte[] pixelBuffer = image.PixelBuffer;
            if (pixelBufferSize > image.PixelBuffer.Length)
            {
                // Allocate a new array that is the correct size.
                pixelBuffer = new byte[pixelBufferSize];

                // Copy in the pixel buffer from the DDS image and pad the remaining bytes.
                Array.Copy(image.PixelBuffer, pixelBuffer, image.PixelBuffer.Length);
                for (int i = image.PixelBuffer.Length; i < pixelBufferSize; i++)
                    pixelBuffer[i] = 0xCD;
            }

            // Create the datastream using the pixel buffer we prepared.
            texture.PixelDataStream = DataStream.Create(pixelBuffer, true, false);

            // Make sure we are at the start of the pixel data stream.
            texture.PixelDataStream.Seek(0, SeekOrigin.Begin);

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

            // Return the texture.
            return texture;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculates the total size of the pixel buffer for all mip maps for a texture with the given parameters
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="faceCount"></param>
        /// <param name="mipCount"></param>
        /// <param name="format"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int CalculatePixelBufferSize(int width, int height, int faceCount, int mipCount, TextureFormat format, TextureType type)
        {
            int pixelBufferSize = 0;

            // Loop for every face in the texture.
            for (int i = 0; i < faceCount; i++)
            {
                // Loop for the number of mip maps per face.
                for (int x = 0; x < mipCount; x++)
                {
                    int bytesPerRow = 0;
                    int numberOfBlocks = 0;

                    // Calculate the pitch values for the current mip level.
                    int mipHeight = type == TextureType.Type_CubeMap ? width : height;
                    CalculateMipMapPitch(width, mipHeight, x, format, out bytesPerRow, out numberOfBlocks);

                    // Update the total pixel buffer size.
                    pixelBufferSize += bytesPerRow * numberOfBlocks;
                }
            }

            // Return the pixel buffer size.
            return pixelBufferSize;
        }

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

        public static int FourCCFromTextureFormat(TextureFormat format, bool isXboxFormat)
        {
            // Check if this is for xbox or not and handle accordingly.
            if (isXboxFormat == false)
            {
                switch (format)
                {
                    case TextureFormat.Format_DXT1: return 0x31545844;
                    case TextureFormat.Format_DXT2: return 0x32545844;
                    case TextureFormat.Format_DXT5: return 0x35545844;
                    case TextureFormat.Format_R8G8_SNORM: return 60;
                    case TextureFormat.Format_B8G8R8A8_UNORM: return 21;
                    default: return -1;
                }
            }
            else
            {
                // Currently not supported.
                throw new NotImplementedException();
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

        #endregion
    }
}
