using DeadRisingArcTool.FileFormats.Archive;
using IO;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry
{
    // sizeof = 8
    public struct rMotionListHeader
    {
        public const int kSizeOf = 8;
        public const int kMagic = 0x544D4C;
        public const int kVersion = 22;

        /* 0x00 */ public int Magic;                // 'TML'
        /* 0x04 */ public short Version;            // 22
        /* 0x06 */ public short AnimationCount;
    }

    // sizeof = 0xD0
    public struct AnimationDescriptor
    {
        public const int kSizeOf = 208;

        /* 0x00 */ public int KeyFrameDataOffset;
        /* 0x04 */ // padding
        /* 0x08 */ public int JointCount; //?
        /* 0x0C */ public int FrameCount;
        /* 0x10 */ public int LoopFrame; //?
        /* 0x14 */ public int Unk1;
        /* 0x18 */ public int Unk2;
        /* 0x1C */ public int Unk3;
        /* 0x20 */ public Vector4 Translation;
        /* 0x30 */ public int Unk4;
        /* 0x34 */ public int Unk5;
        /* 0x38 */ public int Unk6;
        /* 0x3C */ public int Unk7;
        /* 0x40 */ public int Unk8;
        /* 0x44 */ public int Unk9;
        /* 0x48 */ public int Unk10;
        /* 0x4C */ public int Unk11;
        /* 0x50 */ public int Unk12;
        /* 0x54 */ public int Unk13;
        /* 0x58 */ public int Unk14;
        /* 0x5C */ public int Unk15;
        /* 0x60 */ public int Unk16;
        /* 0x64 */ public int Unk17;
        /* 0x68 */ public int Unk18;
        /* 0x6C */ public int Unk19;
        /* 0x70 */ public int Count1;
        /* 0x74 */ // padding
        /* 0x78 */ public int Offset1;      // Entries are 8 bytes, 1 float 1 dword
        /* 0x7C */ // padding
        /* 0x80 */ public int Unk20;
        /* 0x84 */ public int Unk21;
        /* 0x88 */ public int Unk22;
        /* 0x8C */ public int Unk23;
        /* 0x90 */ public int Unk24;
        /* 0x94 */ public int Unk25;
        /* 0x98 */ public int Unk26;
        /* 0x9C */ public int Unk27;
        /* 0xA0 */ public int Unk28;
        /* 0xA4 */ public int Unk29;
        /* 0xA8 */ public int Unk30;
        /* 0xAC */ public int Unk31;
        /* 0xB0 */ public int Unk32;
        /* 0xB4 */ public int Unk33;
        /* 0xB8 */ public int Unk34;
        /* 0xBC */ public int Unk35;
        /* 0xC0 */ public int Count2;
        /* 0xC4 */ // paddinh
        /* 0xC8 */ public int Offset2;
        /* 0xCC */ // padding

        public KeyFrameDescriptor[] KeyFrames;
    }

    // sizeof = 0x18
    public struct KeyFrameDescriptor
    {
        public const int kSizeOf = 24;

        /*
         *  Code:
         *      1: Vector3
         *      2: Vector3
         *      6: 8 bytes, 11-11-10 bit compressed vector3, int? (has flags or something)
         * 
         *  Usage:
         *      4 - seems to be starting position
         */

        /* 0x00 */ public byte Codec;
        /* 0x01 */ public byte Usage; //?
        /* 0x02 */ public byte JointType;
        /* 0x03 */ public byte JointIndex;
        /* 0x04 */ public float Weight; //?
        /* 0x08 */ public int DataSize;
        /* 0x0C */ // padding
        /* 0x10 */ public int DataOffset;
        /* 0x14 */ // padding

        public KeyFrameData[] KeyFrameData;
    }

    public struct KeyFrameData
    {
        public Vector3 Component;
        public int Flags;
    }

    [GameResourceParser(ResourceType.rMotionList)]
    public class rMotionList : GameResource
    {
        public rMotionListHeader header;
        public AnimationDescriptor[] animations;

        public rMotionList(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static rMotionList FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough to hold the header.
            if (buffer.Length < rMotionListHeader.kSizeOf)
                return null;

            // Create a new motion list to populate with info.
            rMotionList motion = new rMotionList(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Parse the header.
            motion.header.Magic = reader.ReadInt32();
            motion.header.Version = reader.ReadInt16();
            motion.header.AnimationCount = reader.ReadInt16();

            // Check the header magic and version.
            if (motion.header.Magic != rMotionListHeader.kMagic || motion.header.Version != rMotionListHeader.kVersion)
            {
                // Header magic or version are invalid or unsupported.
                return null;
            }

            // Read all of the animation descriptor offsets.
            int[] animationDescriptorOffsets = new int[motion.header.AnimationCount];
            for (int i = 0; i < motion.header.AnimationCount; i++)
            {
                animationDescriptorOffsets[i] = reader.ReadInt32();
                reader.BaseStream.Position += 4;
            }

            // Loop through all of the animation descriptors and read each one.
            motion.animations = new AnimationDescriptor[motion.header.AnimationCount];
            for (int i = 0; i < motion.header.AnimationCount; i++)
            {
                // Check if this animation is used.
                if (animationDescriptorOffsets[i] == 0)
                    continue;

                // Seek to the next descriptor and read it.
                reader.BaseStream.Position = animationDescriptorOffsets[i];
                motion.animations[i].KeyFrameDataOffset = reader.ReadInt32();
                reader.BaseStream.Position += 4;
                motion.animations[i].JointCount = reader.ReadInt32();
                motion.animations[i].FrameCount = reader.ReadInt32();
                motion.animations[i].LoopFrame = reader.ReadInt32();
                motion.animations[i].Unk1 = reader.ReadInt32();
                motion.animations[i].Unk2 = reader.ReadInt32();
                motion.animations[i].Unk3 = reader.ReadInt32();
                motion.animations[i].Translation = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                motion.animations[i].Unk4 = reader.ReadInt32();
                motion.animations[i].Unk5 = reader.ReadInt32();
                motion.animations[i].Unk6 = reader.ReadInt32();
                motion.animations[i].Unk7 = reader.ReadInt32();
                motion.animations[i].Unk8 = reader.ReadInt32();
                motion.animations[i].Unk9 = reader.ReadInt32();
                motion.animations[i].Unk10 = reader.ReadInt32();
                motion.animations[i].Unk11 = reader.ReadInt32();
                motion.animations[i].Unk12 = reader.ReadInt32();
                motion.animations[i].Unk13 = reader.ReadInt32();
                motion.animations[i].Unk14 = reader.ReadInt32();
                motion.animations[i].Unk15 = reader.ReadInt32();
                motion.animations[i].Unk16 = reader.ReadInt32();
                motion.animations[i].Unk17 = reader.ReadInt32();
                motion.animations[i].Unk18 = reader.ReadInt32();
                motion.animations[i].Unk19 = reader.ReadInt32();
                motion.animations[i].Count1 = reader.ReadInt32();
                reader.BaseStream.Position += 4;
                motion.animations[i].Offset1 = reader.ReadInt32();
                reader.BaseStream.Position += 4;
                motion.animations[i].Unk20 = reader.ReadInt32();
                motion.animations[i].Unk21 = reader.ReadInt32();
                motion.animations[i].Unk22 = reader.ReadInt32();
                motion.animations[i].Unk23 = reader.ReadInt32();
                motion.animations[i].Unk24 = reader.ReadInt32();
                motion.animations[i].Unk25 = reader.ReadInt32();
                motion.animations[i].Unk26 = reader.ReadInt32();
                motion.animations[i].Unk27 = reader.ReadInt32();
                motion.animations[i].Unk28 = reader.ReadInt32();
                motion.animations[i].Unk29 = reader.ReadInt32();
                motion.animations[i].Unk30 = reader.ReadInt32();
                motion.animations[i].Unk31 = reader.ReadInt32();
                motion.animations[i].Unk32 = reader.ReadInt32();
                motion.animations[i].Unk33 = reader.ReadInt32();
                motion.animations[i].Unk34 = reader.ReadInt32();
                motion.animations[i].Unk35 = reader.ReadInt32();
                motion.animations[i].Count2 = reader.ReadInt32();
                reader.BaseStream.Position += 4;
                motion.animations[i].Offset2 = reader.ReadInt32();

                // Loop and read each key frame.
                motion.animations[i].KeyFrames = new KeyFrameDescriptor[motion.animations[i].JointCount];
                for (int x = 0; x < motion.animations[i].JointCount; x++)
                {
                    // Read the keyframe descriptor.
                    reader.BaseStream.Position = motion.animations[i].KeyFrameDataOffset + (x * KeyFrameDescriptor.kSizeOf);
                    motion.animations[i].KeyFrames[x].Codec = reader.ReadByte();
                    motion.animations[i].KeyFrames[x].Usage = reader.ReadByte();
                    motion.animations[i].KeyFrames[x].JointType = reader.ReadByte();
                    motion.animations[i].KeyFrames[x].JointIndex = reader.ReadByte();
                    motion.animations[i].KeyFrames[x].Weight = reader.ReadSingle();
                    motion.animations[i].KeyFrames[x].DataSize = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    motion.animations[i].KeyFrames[x].DataOffset = reader.ReadInt32();
                    reader.BaseStream.Position += 4;

                    // Save the current position.
                    long position = reader.BaseStream.Position;

                    // Allocate the key frame data array.
                    int entrySizeForCode = KeyFrameDataSizeFromCodec(motion.animations[i].KeyFrames[x].Codec);
                    motion.animations[i].KeyFrames[x].KeyFrameData = new KeyFrameData[motion.animations[i].KeyFrames[x].DataSize / entrySizeForCode];

                    // Seek to the start of the key frame data and read it.
                    reader.BaseStream.Position = motion.animations[i].KeyFrames[x].DataOffset;
                    for (int z = 0; z < motion.animations[i].KeyFrames[x].KeyFrameData.Length; z++)
                    {
                        // Check the codec type and handle accordingly.
                        switch (motion.animations[i].KeyFrames[x].Codec)
                        {
                            case 1:
                            case 2: // Uncompressed vector3
                                {
                                    motion.animations[i].KeyFrames[x].KeyFrameData[z].Component = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                    break;
                                }
                            case 6: // compressed vector3: 17bits per component + flags
                                {
                                    long compressedVec = reader.ReadInt64();
                                    float a = (float)(compressedVec & 0x1FFFF) * 0.000011984317095849380111694336f;
                                    float b = (float)((compressedVec >> 17) & 0x1FFFF) * 0.000011984317095849380111694336f;
                                    float c = (float)((compressedVec >> 34) & 0x1FFFF) * 0.000011984317095849380111694336f;

                                    motion.animations[i].KeyFrames[x].KeyFrameData[z].Component = new Vector3(a, b, c);
                                    motion.animations[i].KeyFrames[x].KeyFrameData[z].Flags = (int)((compressedVec >> 51) & 0x1FFF);
                                    break;
                                }
                            default:
                                {
                                    // Unsupported codec type.
                                    break;
                                }
                        }
                    }
                }
            }

            // Close the binary reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the motion list.
            return motion;
        }

        public static int KeyFrameDataSizeFromCodec(int codec)
        {
            // Check the codec and handle accordingly.
            switch (codec)
            {
                case 1:
                case 2: return 12;
                case 6: return 8;
                default: return 0;
            }
        }
    }
}
