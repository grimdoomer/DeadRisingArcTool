using DeadRisingArcTool.FileFormats.Archive;
using IO;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Text
{
    public struct rMessageHeader
    {
        public const int kMagic = 0x3147534D;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public int DataOffset;
        /* 0x08 */ public int FileSize;
        /* 0x0C */ public int Unk1;
        /* 0x10 */ public short SpriteMapWidth;     // Width of the sprite image
        /* 0x12 */ public short SpriteMapHeight;    // Height of the sprite image
        /* 0x14 */ public short SpriteStrideX;      // Width of a single character sprite
        /* 0x16 */ public short SpriteStrideY;      // Height of a single character sprite
        /* 0x18 */ public int Unk2;
        /* 0x1C */ public short StringCount;
        /* 0x1E */ public byte TerminatorChar;
    }

    public struct CharEntry
    {
        public const int kSizeOf = 6;

        /* 0x00 */ public char Character;   // Ascii character or special case character id
        /* 0x02 */ public short SpriteId;   // Misc value for special case characters, or sprite id for ascii characters
        /* 0x04 */ public byte Width;       // Width of the character
        /* 0x05 */ public byte Flags;

        public CharEntry(char character, short spriteId, byte width, byte flags)
        {
            // Initialize fields.
            this.Character = character;
            this.SpriteId = spriteId;
            this.Width = width;
            this.Flags = flags;
        }

        public bool IsSpecialCharacter()
        {
            // Check for the special character flag.
            return (this.Flags & 4) != 0;
        }
    }

    [GameResourceParser(ResourceType.rMessage)]
    public class rMessage : GameResource
    {
        public rMessageHeader header;
        public CharEntry[][] strings;

        public rMessage(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian) :
            base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static rMessage FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Create a new rMessage object to populate.
            rMessage message = new rMessage(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Read the header and make sure the magic is correct.
            message.header.Magic = reader.ReadInt32();
            message.header.DataOffset = reader.ReadInt32();
            message.header.FileSize = reader.ReadInt32();
            message.header.Unk1 = reader.ReadInt32();
            message.header.SpriteMapWidth = reader.ReadInt16();
            message.header.SpriteMapHeight = reader.ReadInt16();
            message.header.SpriteStrideX = reader.ReadInt16();
            message.header.SpriteStrideY = reader.ReadInt16();
            message.header.Unk2 = reader.ReadInt32();
            message.header.StringCount = reader.ReadInt16();
            message.header.TerminatorChar = reader.ReadByte();

            // Seek to the character data start offset.
            reader.BaseStream.Position = message.header.DataOffset;

            // Allocate the string buffer and read each character entry for every string.
            message.strings = new CharEntry[message.header.StringCount][];
            for (int i = 0; i < message.strings.Length; i++)
            {
                // Create a new list to hold the character entries.
                List<CharEntry> characters = new List<CharEntry>();

                // Loop until we hit the terminator character.
                while (true)
                {
                    // Parse the next character from the stream.
                    CharEntry entry;
                    entry.Character = (char)reader.ReadInt16();
                    entry.SpriteId = reader.ReadInt16();
                    entry.Width = reader.ReadByte();
                    entry.Flags = reader.ReadByte();

                    // Check if this is the terminator character.
                    if (entry.IsSpecialCharacter() == true && entry.Character == message.header.TerminatorChar)
                        break;

                    // Add the character to the list.
                    characters.Add(entry);
                }

                // Save off the character array.
                message.strings[i] = characters.ToArray();
            }

            // Close the reader and return the message data.
            reader.Close();
            return message;
        }

        public Rectangle GetSpriteRect(CharEntry character)
        {
            // Round down the width of the texture to the nearest whole sprite.
            int textureWidth = this.header.SpriteMapWidth;
            if (((textureWidth / this.header.SpriteStrideX) & 1) != 0)
                textureWidth -= this.header.SpriteStrideX;

            // Compute the number of sprites in the width.
            int spriteCount = textureWidth / this.header.SpriteStrideX;

            // Compute the x and y position of the sprite in the texture.
            int xpos = this.header.SpriteStrideX * (character.SpriteId % spriteCount);
            int ypos = this.header.SpriteStrideY * (character.SpriteId / spriteCount);

            // Check for some special flag?
            int newStride = this.header.SpriteStrideX;
            if ((character.Flags & 2) == 0)
                newStride *= 2;

            // Compute the width and height of the sprite.
            int fixedWidth = character.Width > 120 ? 120 : character.Width;
            int width = (int)(((float)fixedWidth * 0.0078125f) * (float)newStride);
            int height = this.header.SpriteStrideY - 1;

            // Return the rectangle.
            return new Rectangle(xpos, ypos, width, height);
        }
    }
}
