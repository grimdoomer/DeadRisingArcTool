using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    public struct r2TextureHeader
    {
        public const int kMagic = 0x00585432;   // 'XT2'
        public const int kVersion = 1;
        public const int kSizeOf = 32;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public byte Version;
    }

    [GameResourceParser(ResourceType.r2Texture)]
    public class r2Texture : GameResource
    {
        public r2TextureHeader header;

        public int unknown;

        public r2Texture(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static r2Texture FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough.
            if (buffer.Length < r2TextureHeader.kSizeOf)
                return null;

            // Create a new texture object to populate with data.
            r2Texture texture = new r2Texture(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Parse the header.
            texture.header.Magic = reader.ReadInt32();
            texture.header.Version = reader.ReadByte();
            reader.BaseStream.Position = 28;
            texture.unknown = reader.ReadInt32();

            // Validate the header.
            if (texture.header.Magic != r2TextureHeader.kMagic || texture.header.Version != r2TextureHeader.kVersion)
            {
                // Texture header is invalid.
                return null;
            }

            // Close the binary reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the texture object.
            return texture;
        }

        #region IRenderable

        public override bool InitializeGraphics(RenderManager manager)
        {
            return true;
        }

        #endregion
    }
}
