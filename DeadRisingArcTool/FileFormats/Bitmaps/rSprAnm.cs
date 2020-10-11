using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.Utilities;
using IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    // sizeof = 0x8
    public struct rSprAnmHeader
    {
        public const int kMagic = 0x4FFC0421;
        public const int kSizeOf = 8;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public short SpriteCount1;
        /* 0x06 */ public short SpriteCount2;
    }

    // sizeof = 0x14
    public struct SpriteBlitInfo
    {
        /* 0x00 */ public byte PosX;        // X position in the texture to start at (must be unsigned)
        /* 0x01 */ public byte PosY;        // Y position in the texture to start at (must be unsigned)	
        /* 0x02 */ public byte Unk1;        // Position for something
        /* 0x03 */ public byte Unk8;        // Position for something
        /* 0x04 */ public byte UnkRunCount1;    // Run count, same as X/YRunCount
        /* 0x04 */ public byte UnkRunCount2;    // Run count, same as X/YRunCount
        /* 0x04 */ public short UnkFlags1;
        [Hex]
        /* 0x06 */ public short Unk3;
        /* 0x08 */ public short Width;
        /* 0x0A */ public short Height;
        /* 0x0C */ public short Unk4;
        /* 0x0E */ public byte XRunCount;   // 2 bits, number of 256 byte runs to add to the xpos
        /* 0x0E */ public byte YRunCount;   // 2 bits, number of 256 byte runs to add to the ypos
        [Hex]
        /* 0x0E */ public byte UnkFlags;
        /* 0x0F */ public byte Unk5;
        [Hex]
        /* 0x10 */ public short Unk6;       // Size for something? upper most bit is a flag
        [Hex]
        /* 0x12 */ public short Unk7;       // Size for something? upper most bit is a flag
    }

    [GameResourceParser(ResourceType.rSprAnm)]
    public class rSprAnm : GameResource
    {
        public rSprAnmHeader header;
        public SpriteBlitInfo[][] blitInfo;

        public rSprAnm(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian) :
            base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static rSprAnm FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is at least large enough to contain the header.
            if (buffer.Length < rSprAnmHeader.kSizeOf)
                return null;

            // Create a new rSprAnm to populate with info.
            rSprAnm sprite = new rSprAnm(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Read the header and make sure the magic is correct.
            sprite.header.Magic = reader.ReadInt32();
            sprite.header.SpriteCount1 = reader.ReadInt16();
            sprite.header.SpriteCount2 = reader.ReadInt16();
            if ((sprite.header.Magic & 0xFFFF) != (rSprAnmHeader.kMagic & 0xFFFF))
            {
                // Sprite has invalid magic.
                reader.Close();
                return null;
            }

            // Read some unknown sprite data.
            if (sprite.header.SpriteCount2 > 0)
            {
                // Not yet supported.
                return null;
            }

            // Read the sprite blit data.
            sprite.blitInfo = new SpriteBlitInfo[sprite.header.SpriteCount1][];
            for (int i = 0; i < sprite.header.SpriteCount1; i++)
            {
                // Read the number of sprite blit entries.
                int entryCount = reader.ReadInt32();

                // Loop for the number of blit info entries.
                sprite.blitInfo[i] = new SpriteBlitInfo[entryCount];
                for (int x = 0; x < entryCount; x++)
                {
                    // Read the blit info struct.
                    sprite.blitInfo[i][x].PosX = reader.ReadByte();
                    sprite.blitInfo[i][x].PosY = reader.ReadByte();
                    sprite.blitInfo[i][x].Unk1 = reader.ReadByte();
                    sprite.blitInfo[i][x].Unk8 = reader.ReadByte();
                    short w = reader.ReadInt16();
                    sprite.blitInfo[i][x].UnkRunCount1 = (byte)(w & 3);
                    sprite.blitInfo[i][x].UnkRunCount2 = (byte)((w >> 2) & 3);
                    sprite.blitInfo[i][x].UnkFlags1 = (short)(w >> 4);
                    sprite.blitInfo[i][x].Unk3 = reader.ReadInt16();
                    sprite.blitInfo[i][x].Width = reader.ReadInt16();
                    sprite.blitInfo[i][x].Height = reader.ReadInt16();
                    sprite.blitInfo[i][x].Unk4 = reader.ReadInt16();
                    byte b = reader.ReadByte();
                    sprite.blitInfo[i][x].XRunCount = (byte)(b & 3);
                    sprite.blitInfo[i][x].YRunCount = (byte)((b >> 2) & 3);
                    sprite.blitInfo[i][x].UnkFlags = (byte)(b >> 4);
                    sprite.blitInfo[i][x].Unk5 = reader.ReadByte();
                    sprite.blitInfo[i][x].Unk6 = reader.ReadInt16();
                    sprite.blitInfo[i][x].Unk7 = reader.ReadInt16();
                }
            }

            // Close the reader and return the sprite.
            reader.Close();
            return sprite;
        }
    }
}
