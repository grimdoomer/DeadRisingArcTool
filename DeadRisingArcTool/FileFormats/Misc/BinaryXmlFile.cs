using DeadRisingArcTool.FileFormats.Archive;
using IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DeadRisingArcTool.FileFormats.Misc
{
    // sizeof = 16
    public struct BinaryXmlHeader
    {
        public const int kSizeOf = 16;
        public const int kMagic = 0x534658;     // 'SFX'
        public const int kVersion = 3;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public short Version;
        /* 0x06 */ public short Unk1;
        /* 0x08 */ public int NodeCount;
        /* 0x0C */ public int NodeDataSize;
    };

    // sizeof = 16
    public struct NodeEntry
    {
        public const int kSizeOf = 16;

        /* 0x00 */ public int ID;
        /* 0x04 */ // padding
        /* 0x08 */ public int ChildNodeCount;
        /* 0x0C */ // padding?

        public NodeDescriptor[] ChildNodes;
    }

    // sizeof = 48
    public struct NodeDescriptor
    {
        public const int kSizeOf = 48;

        /* 0x00 */ public int NodeNameOffset;

        public string NodeName;
    }

    [GameResourceParser(ResourceType.rRouteNode)]
    public class BinaryXmlFile : GameResource
    {
        private BinaryXmlHeader header;
        private NodeEntry[] xmlNodes;

        public XmlDocument Document { get; private set; }

        public BinaryXmlFile(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static BinaryXmlFile FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough to fit the header.
            if (buffer.Length < BinaryXmlHeader.kSizeOf)
                return null;

            // Create a new BinaryXmlFile we can populate with info.
            BinaryXmlFile xmlFile = new BinaryXmlFile(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Parse the header.
            xmlFile.header.Magic = reader.ReadInt32();
            xmlFile.header.Version = reader.ReadInt16();
            xmlFile.header.Unk1 = reader.ReadInt16();
            xmlFile.header.NodeCount = reader.ReadInt32();
            xmlFile.header.NodeDataSize = reader.ReadInt32();

            // Check the header magic and version are correct.
            if (xmlFile.header.Magic != BinaryXmlHeader.kMagic || xmlFile.header.Version != BinaryXmlHeader.kVersion)
            {
                // Header has invalid magic or unsupported version number.
                return null;
            }

            // Read the node data pointers.
            int[] nodeOffsets = new int[xmlFile.header.NodeCount];
            for (int i = 0; i < xmlFile.header.NodeCount; i++)
            {
                // Read the node data offset.
                nodeOffsets[i] = reader.ReadInt32() + BinaryXmlHeader.kSizeOf;
                reader.BaseStream.Position += 4;
            }

            // Allocate the array of child nodes and read each one.
            xmlFile.xmlNodes = new NodeEntry[xmlFile.header.NodeCount];
            for (int i = 0; i < xmlFile.header.NodeCount; i++)
            {
                // Seek to the node entry.
                reader.BaseStream.Position = nodeOffsets[i];

                // Read the next node.
                xmlFile.xmlNodes[i].ID = reader.ReadInt32();
                reader.BaseStream.Position += 4;
                xmlFile.xmlNodes[i].ChildNodeCount = reader.ReadInt32();
                reader.BaseStream.Position += 4;

                // Check if there are child nodes to be read and if so read them.
                if ((xmlFile.xmlNodes[i].ChildNodeCount & 0x7FFF) > 0)
                {
                    // Allocate and read all the node descriptors for this node.
                    xmlFile.xmlNodes[i].ChildNodes = new NodeDescriptor[xmlFile.xmlNodes[i].ChildNodeCount];
                    for (int x = 0; x < xmlFile.xmlNodes[i].ChildNodeCount; x++)
                    {
                        // Seek to the current node entry.
                        reader.BaseStream.Position = nodeOffsets[i] + NodeEntry.kSizeOf + (x * NodeDescriptor.kSizeOf);

                        // Read the next node.
                        xmlFile.xmlNodes[i].ChildNodes[x].NodeNameOffset = reader.ReadInt32() + BinaryXmlHeader.kSizeOf;

                        // Read the name of the node.
                        reader.BaseStream.Position = xmlFile.xmlNodes[i].ChildNodes[x].NodeNameOffset;
                        xmlFile.xmlNodes[i].ChildNodes[x].NodeName = reader.ReadNullTerminatingString();
                    }
                }
            }

            return null;
        }
    }
}
