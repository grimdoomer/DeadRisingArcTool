using IO;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    [Flags]
    public enum ResourceMiscFlags
    {
        DDS_RESOURCE_MISC_TEXTURECUBE = 04,
    }

    public struct DDS_HEADER_DXT10
    {
        public Format dxgiFormat;
        public ResourceDimension resourceDimension;
        public ResourceMiscFlags miscFlag;
        public int arraySize;
        public int miscFlags2;
    }

    [Flags]
    public enum DDPF : int
    {
        DDPF_ALPHAPIXELS = 0x1,
        DDPF_ALPHA = 0x2,
        DDPF_FOURCC = 0x4,
        DDPF_RGB = 0x40,
        DDPF_YUV = 0x200,
        DDPF_LUMINANCE = 0x20000,
        DDPF_BUMPDUDV = 0x80000
    }

    public struct DDS_PIXELFORMAT
    {
        public const int kSizeOf = 32;

        public int dwSize;
        public DDPF dwFlags;
        public int dwFourCC;
        public int dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwABitMask;
    }

    [Flags]
    public enum DDSCAPS : int
    {
        DDSCAPS_COMPLEX = 0x8,
        DDSCAPS_TEXTURE = 0x1000,
        DDSCAPS_MIPMAP = 0x400000
    }

    [Flags]
    public enum DDSCAPS2 : int
    {
        DDSCAPS2_CUBEMAP = 0x200,
        DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
        DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
        DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
        DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
        DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
        DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
        DDSCAPS2_VOLUME = 0x200000,
        DDSCAPS2_CUBEMAP_ALLFACES = DDSCAPS2_CUBEMAP_POSITIVEX | DDSCAPS2_CUBEMAP_NEGATIVEX | 
            DDSCAPS2_CUBEMAP_POSITIVEY | DDSCAPS2_CUBEMAP_NEGATIVEY | DDSCAPS2_CUBEMAP_POSITIVEZ | DDSCAPS2_CUBEMAP_NEGATIVEZ
    }

    [Flags]
    public enum DDSD_FLAGS : int
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
        public int dwSize;              // Size of the header
        public DDSD_FLAGS dwFlags;      // See DDSD_FLAGS
        public int dwHeight;
        public int dwWidth;
        public int dwPitchOrLinearSize;
        public int dwDepth;
        public int dwMipMapCount;
        public int[] dwReserved1; // 11
        public DDS_PIXELFORMAT ddspf;
        public DDSCAPS dwCaps;
        public DDSCAPS2 dwCaps2;
        public int dwCaps3;
        public int dwCaps4;
        public int dwReserved2;
    }

    public class DDSImage
    {
        public DDS_HEADER header;
        private DDS_HEADER_DXT10 dxt10header;

        public byte[] PixelBuffer { get; private set; }

        public DDSD_FLAGS Flags { get { return this.header.dwFlags; } }
        public int Width { get { return this.header.dwWidth; } }
        public int Height { get { return this.header.dwHeight; } }
        public int Depth { get { return this.header.dwDepth; } }
        public int MipMapCount { get { return this.header.dwMipMapCount; } }
        public int Format { get { return this.header.ddspf.dwFourCC; } }
        public DDPF PixelFormatFlags { get { return this.header.ddspf.dwFlags; } }
        public int BitCount { get { return this.header.ddspf.dwRGBBitCount; } }
        public uint RBitMask { get { return this.header.ddspf.dwRBitMask; } }
        public uint GBitMask { get { return this.header.ddspf.dwGBitMask; } }
        public uint BBitMask { get { return this.header.ddspf.dwBBitMask; } }
        public uint ABitMask { get { return this.header.ddspf.dwABitMask; } }
        public DDSCAPS Capabilities { get { return this.header.dwCaps; } }
        public DDSCAPS2 Capabilities2 { get { return this.header.dwCaps2; } }

        protected DDSImage()
        {

        }

        public bool WriteToFile(string fileName)
        {
            EndianWriter writer = null;

            try
            {
                // Open the output file in an EndianWriter.
                writer = new EndianWriter(Endianness.Little, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None));
            }
            catch (Exception e)
            {
                // Failed to open the output file for writing.
                return false;
            }

            // Write the normal DDS header.
            writer.Write(this.header.dwMagic);
            writer.Write(this.header.dwSize);
            writer.Write((int)this.header.dwFlags);
            writer.Write(this.header.dwHeight);
            writer.Write(this.header.dwWidth);
            writer.Write(this.header.dwPitchOrLinearSize);
            writer.Write(this.header.dwDepth);
            writer.Write(this.header.dwMipMapCount);
            writer.WritePadding(11 * 4);
            writer.Write(this.header.ddspf.dwSize);
            writer.Write((int)this.header.ddspf.dwFlags);
            writer.Write(this.header.ddspf.dwFourCC);
            writer.Write(this.header.ddspf.dwRGBBitCount);
            writer.Write(this.header.ddspf.dwRBitMask);
            writer.Write(this.header.ddspf.dwGBitMask);
            writer.Write(this.header.ddspf.dwBBitMask);
            writer.Write(this.header.ddspf.dwABitMask);
            writer.Write((int)this.header.dwCaps);
            writer.Write((int)this.header.dwCaps2);
            writer.Write(this.header.dwCaps3);
            writer.Write(this.header.dwCaps4);
            writer.WritePadding(4);

            // Check if we are using a DX10 header and if so write it to file.
            if (this.header.ddspf.dwFourCC == MakeFourCC("DX10"))
            {
                // Write the dx10 header.
                writer.Write((int)this.dxt10header.dxgiFormat);
                writer.Write((int)this.dxt10header.resourceDimension);
                writer.Write((int)this.dxt10header.miscFlag);
                writer.Write(this.dxt10header.arraySize);
                writer.Write(this.dxt10header.miscFlags2);
            }

            // Write the pixel data to file.
            writer.Write(this.PixelBuffer);

            // Close the file and return.
            writer.Close();
            return true;
        }

        public static DDSImage FromFile(string filePath)
        {
            EndianReader reader = null;

            try
            {
                // Open the file for reading.
                reader = new EndianReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            catch (Exception e)
            {
                // Failed to open the file for reading.
                return null;
            }

            // Check if the file is large enough to contain the header.
            if (reader.BaseStream.Length < DDS_HEADER.kSizeOf)
            {
                // Image is too small to be valid.
                reader.Close();
                return null;
            }

            // Parse the dds image.
            DDSImage image = FromStream(reader);

            // Close the reader and return.
            reader.Close();
            return image;
        }

        public static DDSImage FromBuffer(byte[] data)
        {
            // Create a new memory stream from the buffer.
            using (MemoryStream ms = new MemoryStream(data))
            {
                EndianReader reader = new EndianReader(ms);

                // Parse the dds image from memory.
                DDSImage image = FromStream(reader);

                // Close the reader and return.
                reader.Close();
                return image;
            }
        }

        private static DDSImage FromStream(EndianReader reader)
        {
            // Create a new DDSImage we can populate with info.
            DDSImage image = new DDSImage();
            image.header = new DDS_HEADER();
            image.header.dwMagic = reader.ReadInt32();
            image.header.dwSize = reader.ReadInt32();
            image.header.dwFlags = (DDSD_FLAGS)reader.ReadInt32();
            image.header.dwHeight = reader.ReadInt32();
            image.header.dwWidth = reader.ReadInt32();
            image.header.dwPitchOrLinearSize = reader.ReadInt32();
            image.header.dwDepth = reader.ReadInt32();
            image.header.dwMipMapCount = reader.ReadInt32();
            reader.BaseStream.Position += 11 * 4;
            image.header.ddspf = new DDS_PIXELFORMAT();
            image.header.ddspf.dwSize = reader.ReadInt32();
            image.header.ddspf.dwFlags = (DDPF)reader.ReadInt32();
            image.header.ddspf.dwFourCC = reader.ReadInt32();
            image.header.ddspf.dwRGBBitCount = reader.ReadInt32();
            image.header.ddspf.dwRBitMask = reader.ReadUInt32();
            image.header.ddspf.dwGBitMask = reader.ReadUInt32();
            image.header.ddspf.dwBBitMask = reader.ReadUInt32();
            image.header.ddspf.dwABitMask = reader.ReadUInt32();
            image.header.dwCaps = (DDSCAPS)reader.ReadInt32();
            image.header.dwCaps2 = (DDSCAPS2)reader.ReadInt32();
            image.header.dwCaps3 = reader.ReadInt32();
            image.header.dwCaps4 = reader.ReadInt32();
            reader.BaseStream.Position += 4;

            // Check if the header is valid.
            if (image.header.dwMagic != DDS_HEADER.kMagic || image.header.dwSize != DDS_HEADER.kSizeOf)
            {
                // Image header is invalid.
                reader.Close();
                return null;
            }

            // Check if there is a DX10 header.
            if (image.header.ddspf.dwFourCC == MakeFourCC("DX10"))
            {
                // Read the DX10 header.
                image.dxt10header = new DDS_HEADER_DXT10();
                image.dxt10header.dxgiFormat = (Format)reader.ReadInt32();
                image.dxt10header.resourceDimension = (ResourceDimension)reader.ReadInt32();
                image.dxt10header.miscFlag = (ResourceMiscFlags)reader.ReadInt32();
                image.dxt10header.arraySize = reader.ReadInt32();
                image.dxt10header.miscFlags2 = reader.ReadInt32();
            }

            // Read the pixel buffer.
            image.PixelBuffer = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

            // Return the image.
            return image;
        }

        public static DDSImage FromGameTexture(rTexture texture)
        {
            // Create a new DDSImage to populate with info.
            DDSImage image = new DDSImage();
            image.header = new DDS_HEADER();
            image.header.dwMagic = DDS_HEADER.kMagic;
            image.header.dwSize = DDS_HEADER.kSizeOf;
            image.header.dwFlags = DDSD_FLAGS.DDSD_CAPS | DDSD_FLAGS.DDSD_HEIGHT | DDSD_FLAGS.DDSD_WIDTH | DDSD_FLAGS.DDSD_PIXELFORMAT;// | DDSD_FLAGS.DDSD_MIPMAPCOUNT;
            image.header.dwHeight = texture.Height;
            image.header.dwWidth = texture.Width;
            rTexture.CalculateMipMapPitch(texture.Width, texture.Height, 0, texture.Format, out image.header.dwPitchOrLinearSize, out int blocks);
            image.header.dwDepth = texture.Depth;
            image.header.dwMipMapCount = texture.MipMapCount;
            image.header.ddspf = new DDS_PIXELFORMAT();
            image.header.ddspf.dwSize = DDS_PIXELFORMAT.kSizeOf;
            image.header.ddspf.dwFourCC = FourCCFromTextureFormat(texture.Format);
            image.header.dwCaps = DDSCAPS.DDSCAPS_TEXTURE;

            // If there are more than 1 mip maps or this is a cube map set the mip map flags.
            if (texture.MipMapCount > 0 || texture.Type == TextureType.Type_CubeMap)
            {
                // Set additioanl mip map flags.
                image.header.dwFlags |= DDSD_FLAGS.DDSD_MIPMAPCOUNT;
                image.header.dwCaps |= DDSCAPS.DDSCAPS_MIPMAP | DDSCAPS.DDSCAPS_COMPLEX;

                // Cubemap only flags.
                if (texture.Type == TextureType.Type_CubeMap)
                    image.header.dwCaps2 |= DDSCAPS2.DDSCAPS2_CUBEMAP_ALLFACES;
            }

            // Set additional fields depending on if the texture is a compressed format or not.
            if (texture.Format == TextureFormat.Format_DXT1 || texture.Format == TextureFormat.Format_DXT2 || texture.Format == TextureFormat.Format_DXT5)
            {
                // Pitch field is linear size (pitch of 1 compressed block).
                image.header.dwFlags |= DDSD_FLAGS.DDSD_LINEARSIZE;

                // Set the fourcc format.
                image.header.ddspf.dwFlags |= DDPF.DDPF_FOURCC;
            }
            else
            {
                // Set RGBA bit masks based on the texture format.
                switch (texture.Format)
                {
                    case TextureFormat.Format_B8G8R8A8_UNORM:
                        {
                            image.header.ddspf.dwFlags |= DDPF.DDPF_RGB;
                            image.header.ddspf.dwRGBBitCount = 32;
                            image.header.ddspf.dwRBitMask = 0xFF0000;
                            image.header.ddspf.dwGBitMask = 0xFF00;
                            image.header.ddspf.dwBBitMask = 0xFF;
                            image.header.ddspf.dwABitMask = 0xFF000000;
                            break;
                        }
                    case TextureFormat.Format_R8G8_SNORM:
                        {
                            image.header.ddspf.dwFlags |= DDPF.DDPF_BUMPDUDV;
                            image.header.ddspf.dwRGBBitCount = 16;
                            image.header.ddspf.dwRBitMask = 0xFF;
                            image.header.ddspf.dwGBitMask = 0xFF00;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                //// Setup DX10 header with the correct format value.
                //image.dxt10header = new DDS_HEADER_DXT10();
                //image.dxt10header.dxgiFormat = rTexture.DXGIFromTextureFormat(texture.Format);

                //// Set the resource dimensions based on the texture type.
                //if (texture.Type == TextureType.Type_2D || texture.Type == TextureType.Type_CubeMap)
                //    image.dxt10header.resourceDimension = ResourceDimension.Texture2D;
                //else if (texture.Type == TextureType.Type_DepthMap)
                //    image.dxt10header.resourceDimension = ResourceDimension.Texture3D;

                //// Set special cube map flags.
                //if (texture.Type == TextureType.Type_CubeMap)
                //    image.dxt10header.miscFlag = ResourceMiscFlags.DDS_RESOURCE_MISC_TEXTURECUBE;

                //// TODO: miscFlags2
            }

            // If the texture is a depth map add the depth flag.
            if (texture.Type == TextureType.Type_DepthMap)
                image.header.dwFlags |= DDSD_FLAGS.DDSD_DEPTH;

            // Allocate the pixel buffer.
            image.PixelBuffer = new byte[(int)texture.PixelDataStream.Length];

            // Copy the pixel buffer from the texture.
            texture.PixelDataStream.Seek(0, System.IO.SeekOrigin.Begin);
            texture.PixelDataStream.Read(image.PixelBuffer, 0, image.PixelBuffer.Length);

            //// Check if we need to convert the pixel buffer for none DXT formats.
            //switch (texture.Format)
            //{
            //    case TextureFormat.Format_R8G8_SNORM:
            //        {
            //            image.PixelBuffer = DXTDecoder.DecodeR8G8(image.Width, image.Height, image.PixelBuffer, texture.IsBigEndian);
            //            break;
            //        }
            //    default: break;
            //}

            // Return the DDS image.
            return image;
        }

        public static int FourCCFromTextureFormat(TextureFormat format)
        {
            // Check the format and handle accordingly.
            switch (format)
            {
                case TextureFormat.Format_DXT1: return MakeFourCC("DXT1");
                case TextureFormat.Format_DXT2: return MakeFourCC("DXT3");
                case TextureFormat.Format_DXT5: return MakeFourCC("DXT5");
                default: return 0;// MakeFourCC("DX10");
            }
        }

        public static TextureFormat TextureFormatFromFourCC(int fourcc)
        {
            switch (fourcc)
            {
                case 0x31545844: return TextureFormat.Format_DXT1;
                case 0x32545844: return TextureFormat.Format_DXT2;
                case 0x35545844: return TextureFormat.Format_DXT5;
                default: return TextureFormat.Format_Unsupported;
            }
        }

        /// <summary>
        /// Gets the integer representation of the foucc string.
        /// </summary>
        /// <param name="fourcc"></param>
        /// <returns></returns>
        private static int MakeFourCC(string fourcc)
        {
            return (int)(((int)fourcc[0] & 0xFF) | ((int)fourcc[1] & 0xFF) << 8 | ((int)fourcc[2] & 0xFF) << 16 | ((int)fourcc[3] & 0xFF) << 24);
        }
    }
}
