using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Utilities;
using IO;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;
using BoundingSphere = DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.BoundingSphere;
using BoundingBox = DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.BoundingBox;
using System.Collections;

namespace DeadRisingArcTool.FileFormats.Geometry
{
    // sizeof = 0xA0
    public struct rModelHeader
    {
        public const int kSizeOf = 0xA0;
        public const int kMagic = 0x00444F4D;

        public const int kVersion = 0x70;
        public const int kSubVersion = 1;

        /* 0x00 */ public int Magic;                // 'DOM'
        /* 0x04 */ public byte Version;             // 0x70
        /* 0x05 */ public byte SubVersion;          // 1
        /* 0x06 */ public short JointCount;
        /* 0x08 */ public short PrimitiveCount;
        /* 0x0A */ public short MaterialCount;
        /* 0x0C */ public int VerticeCount;
        /* 0x10 */ public int IndiceCount;
        /* 0x14 */ public int PolygonCount;
        /* 0x18 */ public int VertexData1Size;
        /* 0x1C */ public int VertexData2Size;
        /* 0x20 */ public int NumberOfTextures;
        /* 0x24 */ // padding
        /* 0x28 */ public int JointDataOffset;
        /* 0x2C */ // padding
        /* 0x30 */ public int TextureFilesOffset;
        /* 0x34 */ // padding
        /* 0x38 */ public int PrimitiveDataOffset;
        /* 0x3C */ // padding
        /* 0x40 */ public int VertexData1Offset;
        /* 0x44 */ // padding
        /* 0x48 */ public int VertexData2Offset;
        /* 0x4C */ // padding
        /* 0x50 */ public int IndiceDataOffset;
        /* 0x54 */ // padding
        /* 0x58 */ // LOD?
        /* 0x5C */ // int boundary joint number?
        /* 0x60 */ // vec3 bounding box sphere position?
        /* 0x6C */ public float BoundaryRadius;
        /* 0x70 */ public Vector4 BoundingBoxMin;
        /* 0x80 */ public Vector4 BoundingBoxMax;
        /* 0x90 */ public int MidDist;
        /* 0x94 */ public int LowDist;
        /* 0x98 */ public int LightGroup;
        /* 0x9C */
        /* 0x9D */ // padding
    }

    // sizeof = 0x18
    public struct Joint
    {
        /* 0x00 */ public byte Index;
        /* 0x01 */ public byte ParentIndex;
        /* 0x02 */ public byte[] Padding;   // 6 bytes of padding to align floats
        /* 0x08 */ public float Length;
        /* 0x0C */ public Vector3 Offset;
    }

    // sizeof = 0x40
    public struct BoneMatrix
    {
        /* 0x00 */ public Matrix SRTMatrix;
    }

    // sizeof = 0xD0
    public struct Material
    {
        [Hex]
        /* 0x00 */ public int Flags;            // Upper 5 bits are vertex declaration type (0x14064F550)
	    /* 0x04 */ public int Unk4;             // Flags for what bitmaps are used/how they are used (0x1406B2167)
	    /* 0x08 */ public ShaderTechnique ShaderTechnique;     // Gets set to shader technique index at runtime
        /* 0x0C */ public int Unk5;             // Never read, set on init to shader set index
        [Hex]
        /* 0x10 */ public int Unk6;             // Never read, set to cTrans::VertexDecl pointer on init
        /* 0x14 */ public int Unk7;
        [Hex]
        /* 0x18 */ public int Unk8;             // Checked to be non-zero, then set to a cTrans::VertexDecl pointer
        /* 0x1C */ // padding
	    /* 0x20 */ public int BaseMapTexture;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x24 */ // padding, 0x20 set to 64bit texture object address at runtime
	    /* 0x28 */ public int NormalMapTexture;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x2C */ // padding
	    /* 0x30 */ public int MaskMapTexture;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x34 */ // padding
	    /* 0x38 */ public int LightmapTexture;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x3C */ // padding
	    /* 0x40 */ public int TextureIndex5;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x44 */ // padding
	    /* 0x48 */ public int TextureIndex6;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x4C */ // padding
	    /* 0x50 */ public int TextureIndex7;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x54 */ // padding
	    /* 0x58 */ public int TextureIndex8;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x5C */ // padding
	    /* 0x60 */ public int TextureIndex9;	// texture index, subtract 1 (0 indicates null?)
	    /* 0x64 */ // padding
        /* 0x68 */ public float Transparency;
        [Hex]
        /* 0x6C */ public int Unk11;
        /* 0x70 */ public float FresnelFactor;
        /* 0x74 */ public float FresnelBias;
        /* 0x78 */ public float SpecularPow;
        /* 0x7C */ public float EnvmapPower;    // not sure where I found this name...
        /* 0x80 */ public Vector4 LightMapScale;
        /* 0x90 */ public float DetailFactor;
        /* 0x94 */ public float DetailWrap;
        /* 0x98 */ public float Unk22;
        /* 0x9C */ public float Unk23;
        /* 0xA0 */ public Vector4 Transmit;
        /* 0xB0 */ public Vector4 Parallax;
        /* 0xC0 */ public float Unk32;
        /* 0xC4 */ public float Unk33;
        /* 0xC8 */ public float Unk34;
        /* 0xCC */ public float Unk35;
    }

    // sizeof = 0x50
    public struct Primitive
    {
        /* 0x00 */ public short GroupID;
	    /* 0x02 */ public short	MaterialIndex;
	    /* 0x04 */ public byte Enabled;
	    /* 0x05 */ public byte Unk3;                    // Flags related to draw distance clipping (0x1406B8E50)
	    /* 0x06 */ public byte Unk11;                   // Doesn't seem to be used?
        /* 0x07 */ public byte Unk12;                   // Doesn't seem to be used?
	    /* 0x08 */ public byte VertexStride1;
	    /* 0x09 */ public byte VertexStride2;
        /* 0x0A */ public byte Unk13;                   // Used to enabled/disabled something?
	    /* 0x0B */ // padding
        /* 0x0C */ public int VertexCount;
        /* 0x10 */ public int StartingVertex;
	    /* 0x14 */ public int VertexStream1Offset;      // Passed to CDeviceContext::IASetVertexBuffers
	    /* 0x18 */ public int VertexStream2Offset;	    // Passed to CDeviceContext::IASetVertexBuffers
	    /* 0x1C */ public int StartingIndexLocation;    // Passed to CDeviceContext::DrawIndexed
	    /* 0x20 */ public int IndexCount;               // Passed to CDeviceContext::DrawIndexed
	    /* 0x24 */ public int BaseVertexLocation;       // Passed to CDeviceContext::DrawIndexed
	    /* 0x28 */ // padding to align vectors
	    /* 0x30 */ public Vector4 BoundingBoxMin;
	    /* 0x40 */ public Vector4 BoundingBoxMax;
    }

    public struct AnimatedJoint
    {
        public int JointNumber;
        public int ParentIndex;

        public float Length;
        public int Type;

        public Vector4 Translation;
        public Vector4 Rotation;
        public Vector4 Scale;

        public Vector4 InterpolatedTranslation;
        public Vector4 InterpolatedRotation;
        public Vector4 InterpolatedScale;

        public Matrix SRTMatrix;
    }

    [GameResourceParser(ResourceType.rModel)]
    public class rModel : GameResource
    {
        public rModelHeader header;

        // Joint data is broken into 3 parts per joint.
        public Joint[] joints;
        public BoneMatrix[] jointTranslations;  // ToBone matrix?
        public BoneMatrix[] jointData3;         // ToParent matrix?

        // List of texture files names.
        public string[] textureFileNames;

        // List of materials.
        public Material[] materials;

        // List of primitives.
        public Primitive[] primitives;

        // Vertex and index buffers.
        public ushort[] indexData;
        public byte[] vertexData1;
        public byte[] vertexData2;

        // DirectX resources for rendering.
        private Buffer primaryVertexBuffer = null;
        private Buffer secondaryVertexBuffer = null;
        private Buffer indexBuffer = null;
        private BuiltInShader[] shaders = null;

        private BlendState transparencyBlendState = null;

        private rTexture[] gameTextures = null;
        private Texture2D[] dxTextures = null;
        private ShaderResourceView[] shaderResources = null;

        // Primitive data.
        private BoundingBox[] primitiveBoxes;

        // Bone related data.
        private BoundingSphere[] jointBoundingSpheres;
        private Color4[] boneMatrixData;
        private Texture2D boneMapMatrix;
        private ShaderResourceView boneMapMatrixShaderView;

        // Animation skeleton data.
        private Vector4 baseTranslation = new Vector4(0.0f);
        private AnimatedJoint[] animatedJoints;

        private Vector4 modelPosition = new Vector4(0.0f);
        private Vector4 modelRotation = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        private Vector4 modelScale = new Vector4(1.0f);

        protected rModel(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {

        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        public static rModel FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure the buffer is large enough to hold the header structure.
            if (buffer.Length < rModelHeader.kSizeOf)
                return null;

            // Create a new model object to populate with data.
            rModel model = new rModel(fileName, datum, fileType, isBigEndian);

            // Create a new memory stream and binary reader for the buffer.
            MemoryStream ms = new MemoryStream(buffer);
            EndianReader reader = new EndianReader(isBigEndian == true ? Endianness.Big : Endianness.Little, ms);

            // Parse the header.
            model.header.Magic = reader.ReadInt32();
            model.header.Version = reader.ReadByte();
            model.header.SubVersion = reader.ReadByte();
            model.header.JointCount = reader.ReadInt16();
            model.header.PrimitiveCount = reader.ReadInt16();
            model.header.MaterialCount = reader.ReadInt16();
            model.header.VerticeCount = reader.ReadInt32();
            model.header.IndiceCount = reader.ReadInt32();
            model.header.PolygonCount = reader.ReadInt32();
            model.header.VertexData1Size = reader.ReadInt32();
            model.header.VertexData2Size = reader.ReadInt32();
            model.header.NumberOfTextures = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            model.header.JointDataOffset = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            model.header.TextureFilesOffset = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            model.header.PrimitiveDataOffset = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            model.header.VertexData1Offset = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            model.header.VertexData2Offset = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            model.header.IndiceDataOffset = reader.ReadInt32();
            reader.BaseStream.Position = 0x70;
            model.header.BoundingBoxMin = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0.0f);
            reader.BaseStream.Position += 4;
            model.header.BoundingBoxMax = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0.0f);
            reader.BaseStream.Position += 8;
            model.header.LightGroup = reader.ReadInt32();
            reader.BaseStream.Position = rModelHeader.kSizeOf;

            // Verify the header magic is correct.
            if (model.header.Magic != rModelHeader.kMagic)
            {
                // Header has invalid magic value.
                return null;
            }

            // Check the version and sub version are supported.
            if (model.header.Version != rModelHeader.kVersion || model.header.SubVersion != rModelHeader.kSubVersion)
            {
                // Header has invalid version/subversion numbers.
                return null;
            }

            // Check if there are any joints in the model.
            model.joints = new Joint[model.header.JointCount];
            if (model.header.JointCount > 0)
            {
                // Seek to the joint data offset.
                reader.BaseStream.Position = model.header.JointDataOffset;

                // Read all of the joint meta data.
                for (int i = 0; i < model.header.JointCount; i++)
                {
                    // Read the joint data.
                    model.joints[i] = new Joint();
                    model.joints[i].Index = reader.ReadByte();
                    model.joints[i].ParentIndex = reader.ReadByte();
                    reader.BaseStream.Position += 6;
                    model.joints[i].Length = reader.ReadSingle();
                    model.joints[i].Offset = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

                // Allocate and read all of the joint translations.
                model.jointTranslations = new BoneMatrix[model.header.JointCount];
                for (int i = 0; i < model.header.JointCount; i++)
                {
                    // Read the joint translation.
                    model.jointTranslations[i] = new BoneMatrix();
                    model.jointTranslations[i].SRTMatrix = new Matrix(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

                // Allocate and read all of the joint data 3.
                model.jointData3 = new BoneMatrix[model.header.JointCount];
                for (int i = 0; i < model.header.JointCount; i++)
                {
                    // Read data.
                    model.jointData3[i].SRTMatrix = new Matrix(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }
            }

            // Seek to the texture files offset (must do this even if there are no textures, materials immediately follow this).
            reader.BaseStream.Position = model.header.TextureFilesOffset;

            // Check if there are texture files names to read.
            if (model.header.NumberOfTextures > 0)
            {
                // Loop and read all the texture files names.
                model.textureFileNames = new string[model.header.NumberOfTextures];
                for (int i = 0; i < model.header.NumberOfTextures; i++)
                {
                    // Read the texture string.
                    model.textureFileNames[i] = new string(reader.ReadChars(64)).Trim(new char[] { '\0' });
                }
            }

            // Check if there are materials to read.
            if (model.header.MaterialCount > 0)
            {
                // Allocate and read the material data.
                model.materials = new Material[model.header.MaterialCount];
                for (int i = 0; i < model.header.MaterialCount; i++)
                {
                    // Read the material data.
                    model.materials[i] = new Material();
                    model.materials[i].Flags = reader.ReadInt32();
                    model.materials[i].Unk4 = reader.ReadInt32();
                    model.materials[i].ShaderTechnique = (ShaderTechnique)reader.ReadUInt32();
                    model.materials[i].Unk5 = reader.ReadInt32();
                    model.materials[i].Unk6 = reader.ReadInt32();
                    model.materials[i].Unk7 = reader.ReadInt32();
                    model.materials[i].Unk8 = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].BaseMapTexture = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].NormalMapTexture = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].MaskMapTexture = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].LightmapTexture = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].TextureIndex5 = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].TextureIndex6 = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].TextureIndex7 = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].TextureIndex8 = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].TextureIndex9 = reader.ReadInt32();
                    reader.BaseStream.Position += 4;
                    model.materials[i].Transparency = reader.ReadSingle();
                    model.materials[i].Unk11 = reader.ReadInt32();
                    model.materials[i].FresnelFactor = reader.ReadSingle();
                    model.materials[i].FresnelBias = reader.ReadSingle();
                    model.materials[i].SpecularPow = reader.ReadSingle();
                    model.materials[i].EnvmapPower = reader.ReadSingle();
                    model.materials[i].LightMapScale = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    model.materials[i].DetailFactor = reader.ReadSingle();
                    model.materials[i].DetailWrap = reader.ReadSingle();
                    model.materials[i].Unk22 = reader.ReadSingle();
                    model.materials[i].Unk23 = reader.ReadSingle();
                    model.materials[i].Transmit = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    model.materials[i].Parallax = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    model.materials[i].Unk32 = reader.ReadSingle();
                    model.materials[i].Unk33 = reader.ReadSingle();
                    model.materials[i].Unk34 = reader.ReadSingle();
                    model.materials[i].Unk35 = reader.ReadSingle();
                }
            }

            // Seek to the primitive data offset.
            reader.BaseStream.Position = model.header.PrimitiveDataOffset;

            // Allocate and read the primitive data.
            model.primitives = new Primitive[model.header.PrimitiveCount];
            for (int i = 0; i < model.header.PrimitiveCount; i++)
            {
                // Read the primitive data.
                model.primitives[i] = new Primitive();
                model.primitives[i].GroupID = reader.ReadInt16();
                model.primitives[i].MaterialIndex = reader.ReadInt16();
                model.primitives[i].Enabled = reader.ReadByte();
                model.primitives[i].Unk3 = reader.ReadByte();
                model.primitives[i].Unk11 = reader.ReadByte();
                model.primitives[i].Unk12 = reader.ReadByte();
                model.primitives[i].VertexStride1 = reader.ReadByte();
                model.primitives[i].VertexStride2 = reader.ReadByte();
                model.primitives[i].Unk13 = reader.ReadByte();
                reader.BaseStream.Position += 1;
                model.primitives[i].VertexCount = reader.ReadInt32();
                model.primitives[i].StartingVertex = reader.ReadInt32();
                model.primitives[i].VertexStream1Offset = reader.ReadInt32();
                model.primitives[i].VertexStream2Offset = reader.ReadInt32();
                model.primitives[i].StartingIndexLocation = reader.ReadInt32();
                model.primitives[i].IndexCount = reader.ReadInt32();
                model.primitives[i].BaseVertexLocation = reader.ReadInt32();
                reader.BaseStream.Position += 8;
                model.primitives[i].BoundingBoxMin = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                model.primitives[i].BoundingBoxMax = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            // Check if there is vertex stream 1 data.
            if (model.header.VertexData1Size > 0)
            {
                // Seek to and read the vertex stream 1 data.
                reader.BaseStream.Position = model.header.VertexData1Offset;
                model.vertexData1 = reader.ReadBytes(model.header.VertexData1Size);
            }

            // Check if there is vertex stream 2 data.
            if (model.header.VertexData2Size > 0)
            {
                // Seek to and read the vertex stream 2 data.
                reader.BaseStream.Position = model.header.VertexData2Offset;
                model.vertexData2 = reader.ReadBytes(model.header.VertexData2Size);
            }

            // Seek to the indice buffer offset.
            reader.BaseStream.Position = model.header.IndiceDataOffset;

            // Allocate and read the indice data.
            model.indexData = new ushort[model.header.IndiceCount - 1];
            for (int i = 0; i < model.header.IndiceCount - 1; i++)
            {
                // Read the index data.
                model.indexData[i] = reader.ReadUInt16();
            }

            // Close the binary reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the model object.
            return model;
        }

        private Vector3 GetJointPosition(int index)
        {
            // Check if the joint has a parent and calculate the correct position.
            if (this.joints[index].ParentIndex != 255)
                return this.joints[index].Offset + GetJointPosition(this.joints[index].ParentIndex);
            else
                return this.joints[index].Offset;// + this.baseTranslation.ToVector3();
        }

        private Matrix GetJointTransformation(int index)
        {
            // Check if this joint has a parent and caclulate the correct transformation matric.
            if (this.animatedJoints[index].ParentIndex != 255)
                return this.animatedJoints[index].SRTMatrix * GetJointTransformation(this.animatedJoints[index].ParentIndex);
            else
                return this.animatedJoints[index].SRTMatrix;
        }

        private void UpdateAnimationData(IRenderManager manager, Device device, rMotionList animation)
        {
            // Loop and initialize the animation vectors for every joint in the mesh.
            for (int i = 0; i < this.animatedJoints.Length; i++)
            {
                // See 0x1406B1A80
                this.animatedJoints[i].InterpolatedRotation = Quaternion.RotationMatrix(this.jointTranslations[i].SRTMatrix).ToVector4();
                this.animatedJoints[i].InterpolatedTranslation = new Vector4(this.joints[i].Offset, 1.0f); //new Vector4(new Vector3(0.0f), 1.0f); change for basic models
                this.animatedJoints[i].InterpolatedScale = new Vector4(1.0f);
            }

            // Loop through all the animated joints and update animation data for the current frame.
            int animIndex = manager.GetTime().SelectedAnimation;
            for (int i = 0; i < animation.animations[animIndex].JointCount; i++)
            {
                // Get the key frame descriptor for the current joint.
                KeyFrameDescriptor keyFrameDesc = animation.animations[animIndex].KeyFrames[i];

                // Check if we need to set the joint type and translation.
                if (keyFrameDesc.JointIndex != 255 && keyFrameDesc.JointType != 2 && this.animatedJoints[keyFrameDesc.JointIndex].Type != keyFrameDesc.JointType)
                {
                    // Set the joint type and translation.
                    this.animatedJoints[keyFrameDesc.JointIndex].Type = keyFrameDesc.JointType;
                    this.animatedJoints[keyFrameDesc.JointIndex].Translation = new Vector4(this.joints[keyFrameDesc.JointIndex].Offset, 1.0f);
                }

                // Check the key frame usage and handle accordingly.
                switch (keyFrameDesc.Usage)
                {
                    case 0:
                        {
                            // Joint rotation.
                            this.animatedJoints[keyFrameDesc.JointIndex].InterpolatedRotation = InterpolateKeyFrame(animation, animIndex, i, 
                                manager.GetTime().AnimationCurrentFrame, manager.GetTime().AnimationFrameCount, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                            break;
                        }
                    case 1:
                        {
                            // Joint translation.
                            this.animatedJoints[keyFrameDesc.JointIndex].InterpolatedTranslation = InterpolateKeyFrame(animation, animIndex, i,
                                manager.GetTime().AnimationCurrentFrame, manager.GetTime().AnimationFrameCount, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                            break;
                        }
                    case 2:
                        {
                            // Joint scale.
                            this.animatedJoints[keyFrameDesc.JointIndex].InterpolatedScale = InterpolateKeyFrame(animation, animIndex, i,
                                manager.GetTime().AnimationCurrentFrame, manager.GetTime().AnimationFrameCount, new Vector4(1.0f));
                            break;
                        }
                    case 3:
                        {
                            break;
                        }
                    case 4:
                        {
                            // Root joint translation.
                            this.baseTranslation = InterpolateKeyFrame(animation, animIndex, i, 
                                manager.GetTime().AnimationCurrentFrame, manager.GetTime().AnimationFrameCount, this.baseTranslation);
                            break;
                        }
                }

                // If this is not the root bone then setup child bone types.
                if (keyFrameDesc.JointIndex != 255)
                {
                    // Check joint type and set child bone types for final vector computations.
                    switch (keyFrameDesc.JointType - 3)
                    {
                        case 0:
                        case 1:
                            {
                                this.animatedJoints[keyFrameDesc.JointIndex + 1].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 2].Type = 19;
                                this.animatedJoints[keyFrameDesc.JointIndex + 3].Type = 2;
                                break;
                            }
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            {
                                this.animatedJoints[keyFrameDesc.JointIndex + 1].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 2].Type = 19;
                                break;
                            }
                        case 2:
                        case 3:
                        case 14:
                        case 15:
                        case 18:
                        case 19:
                            {
                                this.animatedJoints[keyFrameDesc.JointIndex + 1].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 2].Type = 20;
                                break;
                            }
                        case 12:
                        case 13:
                            {
                                this.animatedJoints[keyFrameDesc.JointIndex + 1].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 2].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 3].Type = 20;
                                break;
                            }
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            {
                                this.animatedJoints[keyFrameDesc.JointIndex + 1].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 2].Type = 2;
                                this.animatedJoints[keyFrameDesc.JointIndex + 3].Type = 19;
                                break;
                            }
                    }
                }
            }

            // Setup root node animation matrix.
            Matrix rootNodeMatrix = Matrix.RotationQuaternion(this.modelRotation.ToQuaternion());
            rootNodeMatrix.Row4 = new Vector4(this.modelPosition.ToVector3(), 1.0f);

            // Loop through all joints and compute the final animation vectors.
            Vector4 parentJointPosition = new Vector4(0.0f);
            for (int i = 0; i < this.animatedJoints.Length; i++)
            {
                // Check the joint type and handle accordingly.
                switch (this.animatedJoints[i].Type)
                {
                    case 5:
                    case 6:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 21:
                    case 22:
                        {
                            // Check if this joint's parent is the root joint, and if not recursively compute the parent matrix.
                            Matrix parentMatrix = Matrix.Identity;
                            if (this.animatedJoints[i].ParentIndex != 255)
                            {
                                // Loop through the node's parental hierarchy until we hit the root node.
                                for (AnimatedJoint joint = this.animatedJoints[this.animatedJoints[i].ParentIndex]; 
                                    joint.ParentIndex != 255; joint = this.animatedJoints[joint.ParentIndex])
                                {
                                    // Compute the rotational matrix of the current joint and transform the parent joint matrix by it.
                                    Matrix jointMatrix = Matrix.RotationQuaternion(joint.InterpolatedRotation.ToQuaternion());
                                    jointMatrix.TranslationVector = joint.InterpolatedTranslation.ToVector3(); //
                                    parentMatrix *= jointMatrix;
                                }
                            }

                            // Transform the root joint's position by the to-parent matrix.
                            parentJointPosition = Vector4.Transform(new Vector4(parentMatrix.TranslationVector, 1.0f), rootNodeMatrix);
                            break;
                        }
                    case 20:
                        {
                            // Transform the interpolated translation by the root node matrix.
                            Vector3 newTranslation = Vector3.TransformNormal(this.animatedJoints[i].InterpolatedTranslation.ToVector3(), rootNodeMatrix);
                            newTranslation = (newTranslation + rootNodeMatrix.Row4.ToVector3()) - parentJointPosition.ToVector3();

                            // Caclulate the length of the vector.
                            float length = newTranslation.Length();

                            // Normalize the vector and set it as the interpolated translation.
                            newTranslation.Normalize();
                            this.animatedJoints[i].InterpolatedTranslation = new Vector4(newTranslation, 1.0f);

                            // Set the length of the joint.
                            this.animatedJoints[i].Length = length;
                            break;
                        }
                }

                // Set the joint's translation, rotation, and scale.
                this.animatedJoints[i].Rotation = this.animatedJoints[i].InterpolatedRotation;
                this.animatedJoints[i].Translation = this.animatedJoints[i].InterpolatedTranslation;
                this.animatedJoints[i].Scale = this.animatedJoints[i].InterpolatedScale;

                // Calculate the final SRT matrix for the joint.
                this.animatedJoints[i].SRTMatrix = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, this.animatedJoints[i].Scale.ToVector3(),
                    GetJointPosition(i), this.animatedJoints[i].Rotation.ToQuaternion(), this.animatedJoints[i].Translation.ToVector3());

                if (i == 2)
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format("Trans Y: {0}", this.animatedJoints[i].Translation.Y));
                }

                // Update the joint bounding sphere.
                this.jointBoundingSpheres[i].Rotation = this.animatedJoints[i].Rotation;

                //this.animatedJoints[i].SRTMatrix = Matrix.RotationQuaternion(this.animatedJoints[i].Rotation.ToQuaternion());
                //this.animatedJoints[i].SRTMatrix.Column1 *= this.animatedJoints[i].Scale;
                //this.animatedJoints[i].SRTMatrix.Column2 *= this.animatedJoints[i].Scale;
                //this.animatedJoints[i].SRTMatrix.Column3 *= this.animatedJoints[i].Scale;
                //this.animatedJoints[i].SRTMatrix.Column4 = this.animatedJoints[i].Translation;
                //this.animatedJoints[i].SRTMatrix.ScaleVector = this.animatedJoints[i].Scale.ToVector3();
                //this.animatedJoints[i].SRTMatrix.TranslationVector = this.animatedJoints[i].Translation.ToVector3();
            }
        }

        private Vector4 InterpolateKeyFrame(rMotionList animation, int animationIndex, int keyFrameIndex, float frame0, float frame1, Vector4 defaultValue)
        {
            // Setup output vector.
            Vector4 outputVector = defaultValue;

            // Get the key frame descriptor from the animation.
            KeyFrameDescriptor keyFrameDesc = animation.animations[animationIndex].KeyFrames[keyFrameIndex];

            // Check the key frame codec and handle accordingly.
            switch (keyFrameDesc.Codec)
            {
                case 1: // SHOULD BE OKAY
                    {
                        // Check if we are on an even frame boundary and handle accordingly.
                        float framePosition = frame0 - (float)((int)frame0);
                        if (framePosition == 0.0f)
                        {
                            // Even frame boundary.
                            return animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[(int)frame0].Component;
                        }
                        else
                        {
                            // Compute how much time is remaining for this frame.
                            float frameRemainder = 1.0f - framePosition;

                            // Get the starting vector for interpolation.
                            Vector4 vFrameStart = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[(int)frame0].Component;

                            // Check if interpolation will put us past the end of the animation.
                            Vector4 vFrameEnd;
                            if (frame0 + 1.0f < frame1)
                            {
                                // Interpolate with the next frame.
                                vFrameEnd = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[(int)frame0 + 1].Component;
                            }
                            else
                            {
                                // Interpolate with the first frame in the animation (wrap around).
                                vFrameEnd = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[0].Component;
                            }

                            // Interpolate.
                            return (vFrameStart * frameRemainder) + (vFrameEnd * framePosition);
                        }
                    }
                case 2:
                    {
                        // Static position.
                        return animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[0].Component;
                    }
                case 3:
                    {
                        // Get the starting vector for interpolation.
                        Vector4 vFrameStart = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[(int)frame0].Component;

                        // Compute the weight component.
                        float weight = 1.0f - ((vFrameStart.X * vFrameStart.X) + (vFrameStart.Y * vFrameStart.Y) + (vFrameStart.Z * vFrameStart.Z));

                        // Check if we are on an even frame boundary and handle accordingly.
                        float framePosition = frame0 - (float)((int)frame0);
                        if (framePosition == 0.0f)
                        {
                            // Make sure the weight is not negative.
                            if (weight < 0.0f)
                                weight = 0.0f;

                            return new Vector4(vFrameStart.X, vFrameStart.Y, vFrameStart.Z, (float)Math.Sqrt(weight));
                        }
                        else
                        {
                            // Check if interpolation will put us past the end of the animation.
                            Vector4 vFrameEnd;
                            if (frame0 + 1.0f < frame1)
                            {
                                // Interpolate with the next frame.
                                vFrameEnd = framePosition * animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[(int)frame0 + 1].Component;
                            }
                            else
                            {
                                // Interpolate with the first frame in the animation (wrap around).
                                vFrameEnd = framePosition * animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[0].Component;
                            }

                            // Set the scalar components of vFrameStart and vFrameEnd;
                            vFrameStart.W = (float)Math.Sqrt(weight);
                            vFrameEnd.W = (float)Math.Sqrt(1.0f - ((vFrameEnd.X * vFrameEnd.X) + (vFrameEnd.Y * vFrameEnd.Y) + (vFrameEnd.Z * vFrameEnd.Z)));

                            // Compute the dot product as a reference for vector selection.
                            float dotProduct = Vector4.Dot(vFrameStart, vFrameEnd);
                            if (dotProduct < 0.0f)
                                vFrameEnd = 0.0f - vFrameEnd;

                            vFrameEnd = ((vFrameEnd - vFrameStart) * framePosition) + vFrameStart;
                            vFrameEnd.Normalize();
                            return vFrameEnd;
                        }
                    }
                case 4:
                    {
                        // Get the starting vector for interpolation.
                        Vector4 vFrameStart = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[0].Component;

                        // Compute the weight component.
                        float weight = 1.0f - ((vFrameStart.X * vFrameStart.X) + (vFrameStart.Y * vFrameStart.Y) + (vFrameStart.Z * vFrameStart.Z));

                        // Make sure the weight is not negative.
                        if (weight < 0.0f)
                            weight = 0.0f;

                        return new Vector4(vFrameStart.X, vFrameStart.Y, vFrameStart.Z, (float)Math.Sqrt(weight));
                    }
                case 6: // MAYBE OKAY?
                    {
                        // Find the key frame entry for the current frame position.
                        int keyFrameDataIndex = -1;
                        float previousKeyFrameStart = 0.0f;
                        for (int i = 0; i < animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData.Length; i++)
                        {
                            // Check if the current frame falls within this key frame entry.
                            if (frame0 >= previousKeyFrameStart && frame0 < previousKeyFrameStart + (float)animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[i].Duration)
                            {
                                // Current frame number falls in the key frame entry.
                                keyFrameDataIndex = i;
                                break;
                            }

                            // Update keyframe position.
                            previousKeyFrameStart += (float)animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[i].Duration;
                        }

                        // Check if we found a key frame entry.
                        if (keyFrameDataIndex == -1)
                        {
                            // Set the key frame index to the last frame entry, and interpolate with the first key frame entry.
                            keyFrameDataIndex = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData.Length - 1;
                        }

                        // Compute the current position in the frame.
                        float framePosition = (frame0 - previousKeyFrameStart) / animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[keyFrameDataIndex].Duration; //frame0 - (float)((int)frame0);

                        // Compute the frame vector for the key frame entry for frame0.
                        KeyFrameData keyFrame = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[keyFrameDataIndex];
                        Vector4 vCurrentFrame = ComputeCodec6FrameVector(keyFrame);

                        // Check if we need to interpolate with the next key frame, first key frame, or no interpolation is needed.
                        if (keyFrame.Duration == 0)
                        {
                            // Recalculate the frame position.
                            framePosition = frame0 - (float)((int)frame0);

                            // Compute the frame vector for the first frame in the animation.
                            Vector4 vFirstAnimFrame = ComputeCodec6FrameVector(animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[0]);

                            // If the frame position is greater than 0 interpolate with current frame and first frame (wrap around).
                            if (framePosition != 0.0f)
                                return Quaternion.Slerp(new Quaternion(vCurrentFrame), new Quaternion(vFirstAnimFrame), framePosition).ToVector4();
                            else
                                return vCurrentFrame;
                        }
                        else if (framePosition > 0.0f)
                        {
                            // Check if there is another frame in the animation, if not interpolate with the first frame.
                            KeyFrameData nextKeyFrame;
                            if (frame0 + 1.0f < frame1)
                                nextKeyFrame = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[keyFrameDataIndex + 1];
                            else
                                nextKeyFrame = animation.animations[animationIndex].KeyFrames[keyFrameIndex].KeyFrameData[0];

                            // Compute the frame vector for the next frame.
                            Vector4 vNextFrame = ComputeCodec6FrameVector(nextKeyFrame);

                            // Compute the dot product as a reference for vector selection.
                            float dotProduct = Vector4.Dot(vCurrentFrame, vNextFrame);
                            if (dotProduct < 0.0f)
                                vNextFrame = 0.0f - vNextFrame;

                            vNextFrame = ((vNextFrame - vCurrentFrame) * framePosition) + vCurrentFrame;
                            vNextFrame.Normalize();
                            return vNextFrame;
                        }
                        else
                        {
                            // No interpolation needed.
                            return vCurrentFrame;
                        }
                    }
                default:
                    {
                        // Codec not supported.
                        return defaultValue;
                    }
            }
        }

        private Vector4 ComputeCodec6FrameVector(KeyFrameData keyFrame)
        {
            // Calculate the scalar component.
            float scalar = 1.0f - (keyFrame.Scalar * keyFrame.Scalar);
            float sqrtResult = (float)Math.Sqrt(1.0f - (scalar * scalar));

            // Compute the sin estimation of each component in the vector.
            Vector4 vSinEst = keyFrame.Component.SinEst();

            // TODO: verify that x ^ g_XMNegativeZero == -x
            // Compute the final vector components.
            Vector4 vKeyFrame = new Vector4(
                -(vSinEst.X * vSinEst.W * sqrtResult),
                vSinEst.Y * sqrtResult,
                vSinEst.Z * vSinEst.W * sqrtResult,
                scalar);

            // Check the key frame flags and flip x/y/z as needed.
            if ((keyFrame.Flags & 1) != 0)
                vKeyFrame.X = -vKeyFrame.X;
            if ((keyFrame.Flags & 2) != 0)
                vKeyFrame.Y = -vKeyFrame.Y;
            if ((keyFrame.Flags & 4) != 0)
                vKeyFrame.Z = -vKeyFrame.Z;

            // Return the vector.
            return vKeyFrame;
        }

        #region IRenderable

        public override bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Create our vertex and index buffers from the model data.
            this.primaryVertexBuffer = Buffer.Create<byte>(device, BindFlags.VertexBuffer, this.vertexData1);
            if (this.vertexData2 != null)
                this.secondaryVertexBuffer = Buffer.Create<byte>(device, BindFlags.VertexBuffer, this.vertexData2);
            this.indexBuffer = Buffer.Create<ushort>(device, BindFlags.IndexBuffer, this.indexData);

            // Allocate resources for the texture array.
            this.gameTextures = new rTexture[this.header.NumberOfTextures + 1];
            this.dxTextures = new Texture2D[this.header.NumberOfTextures + 1];
            this.shaderResources = new ShaderResourceView[this.header.NumberOfTextures + 1];

            // Loop through all of the textures and setup the directx resources for them.
            for (int i = 0; i < this.dxTextures.Length; i++)
            {
                // First texture is null?
                if (i == 0)
                    continue;

                // Decompress and parse the game resource for this texture.
                this.gameTextures[i] = (rTexture)manager.GetResourceFromFileName(GameResource.GetFullResourceName(this.textureFileNames[i - 1], ResourceType.rTexture));
                if (this.gameTextures[i] != null)
                {
                    // Setup the texture description.
                    Texture2DDescription desc = new Texture2DDescription();
                    desc.Width = this.gameTextures[i].Width;
                    desc.Height = this.gameTextures[i].Height;
                    desc.MipLevels = this.gameTextures[i].MipMapCount;
                    desc.Format = rTexture.DXGIFromTextureFormat(this.gameTextures[i].Format);
                    desc.Usage = ResourceUsage.Default;
                    desc.BindFlags = BindFlags.ShaderResource;
                    desc.SampleDescription.Count = 1;
                    desc.ArraySize = this.gameTextures[i].FaceCount;

                    // Create the texture using the description and resource data we setup.
                    this.dxTextures[i] = new Texture2D(device, desc);
                    device.ImmediateContext.UpdateSubresource(this.gameTextures[i].SubResources[0], this.dxTextures[i]);

                    // Create the shader resource that will use this texture.
                    this.shaderResources[i] = new ShaderResourceView(device, this.dxTextures[i]);
                }
            }

            // Load all the geometry shaders that we might need.
            this.shaders = new BuiltInShader[3];
            this.shaders[0] = manager.GetBuiltInShader(BuiltInShaderType.Game_LevelGeometry2);
            this.shaders[1] = manager.GetBuiltInShader(BuiltInShaderType.Game_Mesh);
            this.shaders[2] = manager.GetBuiltInShader(BuiltInShaderType.Game_LevelGeometry1);

            // Setup the blend state for image transparency.
            BlendStateDescription blendDesc = new BlendStateDescription();
            blendDesc.AlphaToCoverageEnable = false;
            blendDesc.IndependentBlendEnable = false;
            blendDesc.RenderTarget[0].IsBlendEnabled = false;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            this.transparencyBlendState = new BlendState(device, blendDesc);

            // Allocate the primtive bounding box array and initialize each one.
            this.primitiveBoxes = new BoundingBox[this.primitives.Length];
            for (int i = 0; i < this.primitives.Length; i++)
            {
                // Initialize the bounding box.
                this.primitiveBoxes[i] = new BoundingBox(this.primitives[i].BoundingBoxMin, this.primitives[i].BoundingBoxMax, new Color4(0xFFFF0000));
                if (this.primitiveBoxes[i].InitializeGraphics(manager, device) == false)
                {
                    // Failed to initialize the bounding box.
                    return false;
                }
            }

            // Compute the width of the matrix map to the next highest power of 2.
            int matrixMapWidth = NextPowerOfTwo(this.joints.Length);

            // Allocate arrays for joint data.
            this.jointBoundingSpheres = new BoundingSphere[this.joints.Length];
            this.animatedJoints = new AnimatedJoint[this.joints.Length];
            this.boneMatrixData = new Color4[matrixMapWidth * 4];

            // Loop through all the joints and initialize resources for each one.
            for (int i = 0; i < this.joints.Length; i++)
            {
                // Setup the animated joint.
                this.animatedJoints[i].JointNumber = this.joints[i].Index;
                this.animatedJoints[i].ParentIndex = this.joints[i].ParentIndex;
                this.animatedJoints[i].Length = this.joints[i].Length;

                Vector3 jointPosition = GetJointPosition(i);
                this.animatedJoints[i].Translation = new Vector4(jointPosition.X, jointPosition.Y, jointPosition.Z, 1.0f);
                this.animatedJoints[i].Rotation = Quaternion.RotationMatrix(this.jointTranslations[i].SRTMatrix).ToVector4();
                this.animatedJoints[i].Scale = new Vector4(1.0f);
                this.animatedJoints[i].SRTMatrix = this.jointTranslations[i].SRTMatrix;

                // Create the bounding sphere for the current joint.
                this.jointBoundingSpheres[i] = new BoundingSphere(jointPosition, this.animatedJoints[i].Rotation, this.joints[i].Length, new Color4(0xFF00FF00));
                if (this.jointBoundingSpheres[i].InitializeGraphics(manager, device) == false)
                {
                    // Failed to initialize graphics for bounding sphere.
                    return false;
                }
            }

            // Check if there is an animation loaded.
            if (manager.GetMotionList() != null)
            {
                // Get the loaded animation.
                rMotionList animation = manager.GetMotionList();

                // Set the selected animation.
                int selectedAnimation = 0;
                manager.GetTime().SelectedAnimation = selectedAnimation;
                manager.GetTime().AnimationFrameRate = 15.0f;
                manager.GetTime().AnimationTimePerFrame = 1.0f / manager.GetTime().AnimationFrameRate;
                manager.GetTime().AnimationTotalTime = manager.GetTime().AnimationTimePerFrame * animation.animations[selectedAnimation].FrameCount;
                manager.GetTime().AnimationCurrentTime = 0.0f;
                manager.GetTime().AnimationFrameCount = animation.animations[selectedAnimation].FrameCount;
                manager.GetTime().AnimationCurrentFrame = 0.0f;
            }

            {
                // Setup the bone map matrix bitmap description, we want a 1x(joint count * 4) image to hold the matrices.
                Texture2DDescription desc = new Texture2DDescription();
                desc.Width = matrixMapWidth * 4;
                desc.Height = 1;
                desc.ArraySize = 1;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.Usage = ResourceUsage.Default;
                desc.SampleDescription.Count = 1;
                desc.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                desc.MipLevels = 1;

                // Create the texture and update it's pixel buffer with the matrix data.
                this.boneMapMatrix = new Texture2D(device, desc);

                // Create the shader resource view that will bind this texture.
                this.boneMapMatrixShaderView = new ShaderResourceView(device, this.boneMapMatrix);
            }

            // Successfully initialized.
            return true;
        }

        public override bool DrawFrame(IRenderManager manager, Device device)
        {
            // Check if there is an animation loaded and if so update.
            rMotionList animation = manager.GetMotionList();
            if (animation != null)
            {
                // Get the input manager and check for animation button input.
                InputManager input = manager.GetInputManager();
                if (input.ButtonPressed(InputAction.NextAnimation) == true || input.ButtonPressed(InputAction.PreviousAnimation) == true)
                {
                    // Handle input accordingly.
                    if (input.ButtonPressed(InputAction.NextAnimation) == true)
                    {
                        // Increment or wrap the selected animation index.
                        if (++manager.GetTime().SelectedAnimation >= animation.animations.Length)
                            manager.GetTime().SelectedAnimation = 0;
                    }
                    else
                    {
                        // Decrement or wrap the selected animation index.
                        if (--manager.GetTime().SelectedAnimation < 0)
                            manager.GetTime().SelectedAnimation = animation.animations.Length - 1;
                    }

                    // Reset position and time counters.
                    manager.GetTime().AnimationTotalTime = manager.GetTime().AnimationTimePerFrame * animation.animations[manager.GetTime().SelectedAnimation].FrameCount;
                    manager.GetTime().AnimationCurrentTime = 0.0f;
                    manager.GetTime().AnimationFrameCount = animation.animations[manager.GetTime().SelectedAnimation].FrameCount;
                    manager.GetTime().AnimationCurrentFrame = 0.0f;
                }

                // Update the current animation frame counter.
                manager.GetTime().AnimationCurrentTime += manager.GetTime().TimeDelta;
                manager.GetTime().AnimationCurrentFrame = manager.GetTime().AnimationCurrentTime / manager.GetTime().AnimationTimePerFrame;

                // Check if we need to reset the frame counters.
                if (manager.GetTime().AnimationCurrentTime >= manager.GetTime().AnimationTotalTime)
                {
                    // Reset the frame and time counters.
                    manager.GetTime().AnimationCurrentTime = 0.0f;
                    manager.GetTime().AnimationCurrentFrame = manager.GetTime().AnimationCurrentTime / manager.GetTime().AnimationTimePerFrame;
                }

                // Update animation data for all joints.
                UpdateAnimationData(manager, device, animation);
            }

            // Loop through all of the joints and update the bone matrix data.
            for (int i = 0; i < this.animatedJoints.Length; i++)
            {
                // Copy the translation matrix into the bone matrix buffer.
                Matrix SRTMatrix = GetJointTransformation(i);
                boneMatrixData[i * 4] = new Color4(SRTMatrix.Column1);
                boneMatrixData[i * 4 + 1] = new Color4(SRTMatrix.Column2);
                boneMatrixData[i * 4 + 2] = new Color4(SRTMatrix.Column3);
                boneMatrixData[i * 4 + 3] = new Color4(SRTMatrix.Column4);
            }

            // Update the bone matrix map texture.
            device.ImmediateContext.UpdateSubresource(this.boneMatrixData, this.boneMapMatrix, rowPitch: 16 * 4);

            // Set the primitive type.
            device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            // Set alpha blending state.
            device.ImmediateContext.OutputMerger.SetBlendState(this.transparencyBlendState, new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f));

            // Loop through all of the primitives for the model and draw each one.
            for (int i = 0; i < this.primitives.Length; i++)
            {
                // Check if the primitive is enabled.
                if (this.primitives[i].Enabled == 0)
                    continue;

                // Set the vertex and index buffers.
                device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(this.primaryVertexBuffer, this.primitives[i].VertexStride1, this.primitives[i].VertexStream1Offset),
                    new VertexBufferBinding(this.secondaryVertexBuffer, this.primitives[i].VertexStride2, this.primitives[i].VertexStream2Offset));
                device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

                // Check the flags on the material for this primitive to determine what shader to render with.
                int shaderFlags = (this.materials[this.primitives[i].MaterialIndex].Flags >> 27) & 7;
                switch (shaderFlags)
                {
                    case 0: this.shaders[1].DrawFrame(manager, device); break;
                    case 1: continue; break;
                    case 2: this.shaders[2].DrawFrame(manager, device); break; 
                }

                // Get the material for the primitive.
                Material material = this.materials[this.primitives[i].MaterialIndex];

                // Set the textures being used by the material.
                device.ImmediateContext.PixelShader.SetShaderResource(0, this.shaderResources[material.BaseMapTexture]);

                // Set the bone map matrix data.
                float matrixUnitSize = 1.0f / (float)NextPowerOfTwo(this.joints.Length);
                float matrixRowSize = matrixUnitSize / 4.0f;
                manager.SetMatrixMapFactor(new Vector4(0.0f, 0.0f, matrixUnitSize, matrixRowSize));
                device.ImmediateContext.VertexShader.SetShaderResource(0, this.boneMapMatrixShaderView);

                // Draw the primtive.
                device.ImmediateContext.DrawIndexed(this.primitives[i].IndexCount, this.primitives[i].StartingIndexLocation, this.primitives[i].BaseVertexLocation);
            }

            // Get the debug draw flags and check if we should draw extra stuff.
            DebugDrawOptions options = manager.GetDebugDrawOptions();

            // Joint bounding speheres:
            if (options.HasFlag(DebugDrawOptions.DrawJointBoundingSpheres) == true)
            {
                // Loop and draw bounding sphere for all the joints in the mesh.
                for (int i = 0; i < this.jointBoundingSpheres.Length; i++)
                {
                    // Draw the bounding sphere.
                    this.jointBoundingSpheres[i].DrawFrame(manager, device);
                }
            }

            // Primitive bounding boxes:
            if (options.HasFlag(DebugDrawOptions.DrawPrimitiveBoundingBox) == true)
            {
                // Loop and draw the bounding boxes for all the primitives in the mesh.
                for (int i = 0; i < this.primitiveBoxes.Length; i++)
                {
                    // Draw the bounding box.
                    this.primitiveBoxes[i].DrawFrame(manager, device);
                }
            }

            // Done rendering.
            return true;
        }

        public override void CleanupGraphics(IRenderManager manager, Device device)
        {
            // TODO:
        }

        #endregion

        private static int NextPowerOfTwo(int value)
        {
            int msb = 0;

            // Get the highest bit set in the value.
            for (int i = 0; i < 31; i++)
            {
                if ((value & (1 << i)) != 0 && i > msb)
                    msb = i;
            }

            // Return the next highest power of 2.
            return 1 << (msb + 1);
        }
    }
}
