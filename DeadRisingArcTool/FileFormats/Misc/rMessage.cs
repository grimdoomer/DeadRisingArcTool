using DeadRisingArcTool.FileFormats.Archive;
using IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Misc
{
    public struct rMessageHeader
    {
        public const int kMagic = 0x3147534D;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public int DataOffset;
        /* 0x08 */ public int FileSize;

        /* 0x1C */ public short StringCount;
        /* 0x1E */ public byte TerminatorChar;
    }

    public struct CharEntry
    {
        public const int kSizeOf = 6;

        /* 0x00 */ public byte Character;   // Ascii character or special case character id
        /* 0x01 */ public byte Unk1;        //
        /* 0x02 */ public short Unk2;       // Misc value for special case characters, or sprite id for ascii characters
        /* 0x04 */ public byte Width;       // Width of the character
        /* 0x05 */ public byte Flags;
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
            reader.BaseStream.Position = 0x1C;
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
                    entry.Character = reader.ReadByte();
                    entry.Unk1 = reader.ReadByte();
                    entry.Unk2 = reader.ReadInt16();
                    entry.Width = reader.ReadByte();
                    entry.Flags = reader.ReadByte();

                    // Check if this is the terminator character.
                    if ((entry.Flags & 4) != 0 && entry.Character == message.header.TerminatorChar)
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
    }
}
