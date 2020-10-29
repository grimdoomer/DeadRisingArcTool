using DeadRisingArcTool.FileFormats.Archive;
using IO;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /* 0x06 */ public short SubVersion;
        /* 0x08 */ public int NodeCount;
        /* 0x0C */ public int NodeDataSize;
    };

    // sizeof = 16
    public struct NodeEntry
    {
        public const int kSizeOf = 16;

        /* 0x00 */ public uint TypeId;
        /* 0x04 */ // padding
        /* 0x08 */ public int ChildNodeCount;
        /* 0x0C */ // padding

        public NodeDescriptor[] ChildNodes;
    }

    // sizeof = 48
    public struct NodeDescriptor
    {
        public const int kSizeOf = 48;

        /* 0x00 */ public int NodeNameOffset;
        /* 0x04 */ // padding
        /* 0x08 */ public byte PropertyType;
        /* 0x08 */ public uint Flags;
        // Runtime only fields...

        public string NodeName;
    }

    [GameResourceParser(ResourceType.rRouteNode,
                        ResourceType.rEnemyLayout,
                        ResourceType.rUBCell,
                        ResourceType.rSoundSeg)]
    public class BinaryXmlFile : GameResource
    {
        private BinaryXmlHeader header;
        private NodeEntry[] xmlNodes;

        public object ParsedObject { get; set; }

        // List of all SerializableXmlStruct structures in the code base, only needs to be built once per session.
        private static Dictionary<uint, Type> SerializableStructTypes = Assembly.GetExecutingAssembly().GetTypes().Where(
            t => t.GetCustomAttribute(typeof(SerializableXmlStructAttribute)) != null).ToDictionary<Type, uint>(t => t.GetCustomAttribute<SerializableXmlStructAttribute>().Type);

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
            xmlFile.header.SubVersion = reader.ReadInt16();
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
                xmlFile.xmlNodes[i].TypeId = reader.ReadUInt32();
                reader.BaseStream.Position += 4;
                xmlFile.xmlNodes[i].ChildNodeCount = reader.ReadInt32();
                reader.BaseStream.Position += 4;

                // Check if there are child nodes to be read and if so read them.
                if ((xmlFile.xmlNodes[i].ChildNodeCount & 0x7FFF) > 0)
                {
                    // Allocate and read all the node descriptors for this node.
                    xmlFile.xmlNodes[i].ChildNodes = new NodeDescriptor[xmlFile.xmlNodes[i].ChildNodeCount];
                    for (int x = 0; x < (xmlFile.xmlNodes[i].ChildNodeCount & 0x7FFF); x++)
                    {
                        // Seek to the current node entry.
                        reader.BaseStream.Position = nodeOffsets[i] + NodeEntry.kSizeOf + (x * NodeDescriptor.kSizeOf);

                        // Read the next node.
                        xmlFile.xmlNodes[i].ChildNodes[x].NodeNameOffset = reader.ReadInt32() + BinaryXmlHeader.kSizeOf;
                        reader.BaseStream.Position += 4;
                        xmlFile.xmlNodes[i].ChildNodes[x].Flags = reader.ReadUInt32();
                        xmlFile.xmlNodes[i].ChildNodes[x].PropertyType = (byte)(xmlFile.xmlNodes[i].ChildNodes[x].Flags & 0xFF);

                        // Read the name of the node.
                        reader.BaseStream.Position = xmlFile.xmlNodes[i].ChildNodes[x].NodeNameOffset;
                        xmlFile.xmlNodes[i].ChildNodes[x].NodeName = reader.ReadNullTerminatingString();
                    }
                }
            }

            // Seek to the start of the xml value data.
            reader.BaseStream.Position = xmlFile.header.NodeDataSize + BinaryXmlHeader.kSizeOf;

            // Parse the xml tree.
            xmlFile.ParsedObject = ParseXmlTree(xmlFile, reader);

            // Close the reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the parsed xml file.
            return xmlFile;
        }

        private static object ParseXmlTree(BinaryXmlFile xmlFile, EndianReader reader)
        {
            // Read the object descriptor id.
            uint objectId = reader.ReadUInt32();
            if ((objectId & 0xFFFE) == 0xFFFE)
                return null;

            // Get the object descriptor for this object id.
            int objectIndex = (int)((objectId >> 1) & 0x7FFF);
            NodeEntry objectDesc = xmlFile.xmlNodes[objectIndex];

            // Make sure we have a structure tied to this resource type.
            if (SerializableStructTypes.ContainsKey(objectDesc.TypeId) == false)
            {
                // Write the xml schema to file for analysis.
                string filePath = string.Format("{0}\\{1}.txt", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                    xmlFile.FileType.ToString().Replace("ResourceType.", ""));
                xmlFile.DumpSchema(filePath);

                //return null;
                throw new NotImplementedException(string.Format("Unsupported structure type {0}!", objectDesc.TypeId.ToString("X")));
            }

            // Create an object that matches the resource type to populate with info.
            Type objectType = SerializableStructTypes[objectDesc.TypeId];
            object parsedObject = Activator.CreateInstance(objectType);

            // Get a list of all fields that have a XmlField attribute attached to them.
            Dictionary<string, FieldInfo> xmlFields = objectType.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(
                f => f.GetCustomAttribute(typeof(XmlFieldAttribute)) != null).ToDictionary<FieldInfo, string>(f => f.GetCustomAttribute<XmlFieldAttribute>().FieldName);

            // Check for some bit in the object id.
            if ((objectId & 1) == 0)
                return null;

            // Read the node data size.
            long nodeDataSize = reader.ReadInt64();

            // Loop through all of the fields in the object and parse each one.
            for (int i = 0; i < (objectDesc.ChildNodeCount & 0x7FFF); i++)
            {
                // Check for some flag (not sure if this exists in file form).
                if ((objectDesc.ChildNodes[i].Flags & 0x80000000) != 0)
                {

                }

                // Check for some other flag (not sure if this exists in file form.
                if ((objectDesc.ChildNodes[i].Flags & 0x8000) != 0)
                {

                }

                // Check if we need to read the object count from the stream or not.
                int objectCount = 1;
                if ((objectDesc.ChildNodes[i].Flags & 0x2000) != 0)
                {
                    // Read the object count from the stream.
                    objectCount = reader.ReadInt32();
                }
                
                // Loop for the object count and read each element from the stream.
                object[] value = new object[objectCount];
                for (int x = 0; x < objectCount; x++)
                {
                    // Check the field type and handle accordingly.
                    switch (objectDesc.ChildNodes[i].PropertyType)
                    {
                        case 1:
                            {
                                value[x] = ParseXmlTree(xmlFile, reader);
                                break;
                            }
                        case 2:
                            {
                                value[x] = ParseXmlTree(xmlFile, reader);
                                break;
                            }
                        case 3: value[x] = !(reader.ReadByte() == 0); break;
                        case 4: value[x] = reader.ReadByte(); break;
                        case 5: value[x] = reader.ReadUInt16(); break;
                        case 6: value[x] = reader.ReadUInt32(); break;
                        case 7: value[x] = reader.ReadUInt64(); break;
                        case 8: value[x] = reader.ReadSByte(); break;
                        case 9: value[x] = reader.ReadInt16(); break;
                        case 10: value[x] = reader.ReadInt32(); break;
                        case 11: value[x] = reader.ReadInt64(); break;
                        case 12: value[x] = reader.ReadSingle(); break;
                        case 20: value[x] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()); reader.BaseStream.Position += 4; break;
                        case 22: value[x] = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()); break;

                        case 14:
                        case 23:
                        case 30:
                        case 54:
                            {
                                break;
                            }
                        default:
                            {
                                // Unsupported field type.
                                break;
                            }
                    }
                }

                // Make sure there is a field in the object with this field name.
                if (xmlFields.ContainsKey(objectDesc.ChildNodes[i].NodeName) == false)
                {
                    // Object does not contain a field tied to the specified field name.
                    throw new NotImplementedException();
                }

                // Set the field value to what we just read.
                if (xmlFields[objectDesc.ChildNodes[i].NodeName].FieldType.IsArray == false)
                    xmlFields[objectDesc.ChildNodes[i].NodeName].SetValue(parsedObject, value[0]);
                else
                {
                    // Get the field type so we can create an array of it.
                    Type fieldType = xmlFields[objectDesc.ChildNodes[i].NodeName].FieldType;
                    Array arrayObj = (Array)Activator.CreateInstance(fieldType, new object[] { objectCount });

                    // Loop and fill the array with the values we read.
                    for (int x = 0; x < objectCount; x++)
                        arrayObj.SetValue(value[x], x);

                    // Set the field value in the parsed object to the array we just created.
                    xmlFields[objectDesc.ChildNodes[i].NodeName].SetValue(parsedObject, arrayObj);
                }
            }

            // Return the parsed object.
            return parsedObject;
        }

        private void DumpSchema(string filePath)
        {
            // Create a new stream writer to write the schema file with.
            StreamWriter writer = new StreamWriter(filePath);

            // Loop through all of the type descriptors and write each one to file.
            for (int i = 0; i < this.xmlNodes.Length; i++)
            {
                // Write the structure header.
                writer.WriteLine(string.Format("[SerializableXmlStruct(0x{0})]", this.xmlNodes[i].TypeId.ToString("X")));
                writer.WriteLine(string.Format("public struct Node_{0}", this.xmlNodes[i].TypeId.ToString("X")));
                writer.WriteLine("{");

                // Loop through all of the child fields and write each one.
                for (int x = 0; x < this.xmlNodes[i].ChildNodeCount; x++)
                {
                    // Write the xml field attribute.
                    writer.WriteLine(string.Format("\t[XmlField(\"{0}\")]", this.xmlNodes[i].ChildNodes[x].NodeName));

                    // Sanitize the field name.
                    string fieldName = this.xmlNodes[i].ChildNodes[x].NodeName.Replace("*", "").Replace(' ', '_');

                    // Check the field type and handle accordingly.
                    switch (this.xmlNodes[i].ChildNodes[x].PropertyType)
                    {
                        case 1:
                        case 2:
                            {
                                // Array of some object type that we can not determine from the field descriptors alone.
                                writer.WriteLine(string.Format("\tpublic object[] {0};", fieldName));
                                break;
                            }
                        case 3: writer.WriteLine(string.Format("\tpublic bool {0};", fieldName)); break;
                        case 4: writer.WriteLine(string.Format("\tpublic byte {0};", fieldName)); break;
                        case 5: writer.WriteLine(string.Format("\tpublic ushort {0};", fieldName)); break;
                        case 6: writer.WriteLine(string.Format("\tpublic uint {0};", fieldName)); break;
                        case 7: writer.WriteLine(string.Format("\tpublic ulong {0};", fieldName)); break;
                        case 8: writer.WriteLine(string.Format("\tpublic sbyte {0};", fieldName)); break;
                        case 9: writer.WriteLine(string.Format("\tpublic short {0};", fieldName)); break;
                        case 10: writer.WriteLine(string.Format("\tpublic int {0};", fieldName)); break;
                        case 11: writer.WriteLine(string.Format("\tpublic long {0};", fieldName)); break;
                        case 12: writer.WriteLine(string.Format("\tpublic float {0};", fieldName)); break;
                        case 20: writer.WriteLine(string.Format("\tpublic Vector3 {0};", fieldName)); break;
                            //
                        case 22: writer.WriteLine(string.Format("\tpublic Quaternion {0};", fieldName)); break;

                        case 14:
                        case 23:
                        case 30:
                        case 54:
                            {
                                break;
                            }
                        default:
                            {
                                // Unsupported field type.
                                break;
                            }
                    }
                }

                // Write the structure footer.
                writer.WriteLine("}\r\n");
            }

            // Close the stream writer.
            writer.Close();
        }
    }
}
