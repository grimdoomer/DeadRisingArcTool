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
using Buffer = SharpDX.Direct3D11.Buffer;
using BoundingSphere = DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.BoundingSphere;
using BoundingBox = DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.BoundingBox;
using ImGuiNET;
using ImVector2 = System.Numerics.Vector2;
using System.Runtime.InteropServices;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.UI;

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
        //[Hex]
        /* 0x6C */ public float Unk11;
        /* 0x70 */ public float FresnelFactor;
        /* 0x74 */ public float FresnelBias;
        /* 0x78 */ public float SpecularPow;
        /* 0x7C */ public float EnvmapPower;
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
        private Shader[] shaders = null;

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

        // Animation file being played on the model.
        private rMotionList activeAnimation = null;

        // Animation timing data.
        private int selectedAnimation;               // Index of the animation that is currently playing
        private float animationPlaybackRate = 1.0f;  // Speed to playback the animation at
        private float animationFrameRate;            // Playback rate of the animation i.e.: 10fps
        private float animationTimePerFrame;         // Time per frame of the animation
        private float animationTotalTime;            // Total time the animation will take to complete
        private float animationCurrentTime;          // Time of the current position in animation playback
        private float animationFrameCount;           // Number of frames in the animation
        private float animationCurrentFrame;         // Frame number of the current position in animation playback

        private bool animationPaused = false;       // Indicates if the current animation is paused or not

        // Animation skeleton data.
        private Vector4 baseTranslation = new Vector4(0.0f);
        private AnimatedJoint[] animatedJoints;

        public Vector4 modelPosition = new Vector4(0.0f);
        public Vector4 modelRotation = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public Vector4 modelScale = new Vector4(1.0f);

        // UI data.
        private int[] primtiveGroupIds = null;
        private Dictionary<int, SortedSet<int>> primitivesByGroup = new Dictionary<int, SortedSet<int>>();
        private bool[] visibleGroupIds = null;

        private int selectedMaterialIndex = 0;

        private int selectedPrimitiveIndex = -1;
        private int hoveredPrimitiveIndex = -1;
        private bool[] visiblePrimitives = null;

        private bool showJointSpheres = false;
        private bool showBoundingBoxes = false;

        // File selection dialog data.
        private ImGuiResourceSelectDialog fileSelectDialog = null;

        protected rModel(string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {

        }

        #region GameResource Functions

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
                    model.materials[i].Unk11 = reader.ReadSingle();
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

        #endregion

        #region Animation playback

        private Vector3 GetJointPosition(int index)
        {
            // Check if the joint has a parent and calculate the correct position.
            if (this.joints[index].ParentIndex != 255)
                return this.joints[index].Offset + GetJointPosition(this.joints[index].ParentIndex);
            else
                return this.joints[index].Offset + this.baseTranslation.ToVector3();
        }

        private Vector3 GetAnimatedJointPosition(int index)
        {
            // Check if the joint has a parent and calculate the correct position.
            if (this.animatedJoints[index].ParentIndex != 255)
                return this.animatedJoints[index].Translation.ToVector3() + GetJointPosition(this.animatedJoints[index].ParentIndex);
            else
                return this.animatedJoints[index].Translation.ToVector3() + this.baseTranslation.ToVector3();
        }

        private Matrix GetJointTransformation(int index)
        {
            // Check if this joint has a parent and caclulate the correct transformation matric.
            if (this.animatedJoints[index].ParentIndex != 255)
                return this.animatedJoints[index].SRTMatrix * GetJointTransformation(this.animatedJoints[index].ParentIndex);
            else
                return this.animatedJoints[index].SRTMatrix;
        }

        private void UpdateAnimationData(RenderManager manager, rMotionList animation)
        {
            // Loop and initialize the animation vectors for every joint in the mesh.
            for (int i = 0; i < this.animatedJoints.Length; i++)
            {
                Vector4 parentOffset = new Vector4(0.0f);
                if (this.animatedJoints[i].ParentIndex != 255)
                    parentOffset = this.animatedJoints[this.animatedJoints[i].ParentIndex].Translation;

                // See 0x1406B1A80
                this.animatedJoints[i].InterpolatedRotation = Quaternion.RotationMatrix(this.jointTranslations[i].SRTMatrix).ToVector4(); //new Vector4(0.0f, 0.0f, 0.0f, 1.0f);// 
                this.animatedJoints[i].InterpolatedTranslation = new Vector4(new Vector3(0.0f), 1.0f); // this.animatedJoints[i].Translation; //change for basic models 
                this.animatedJoints[i].InterpolatedScale = new Vector4(1.0f);
                //this.animatedJoints[i].SRTMatrix = this.jointData3[i].SRTMatrix;
            }

            // Loop through all of the animated joints and setup the joint types.
            for (int i = 0; i < animation.animations[this.selectedAnimation].JointCount; i++)
            {
                // Get the key frame descriptor for the current joint.
                KeyFrameDescriptor keyFrameDesc = animation.animations[this.selectedAnimation].KeyFrames[i];

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

            // Loop through all of the key frames and process any for the root joint.
            if (animation.animations[this.selectedAnimation].KeyFrames != null)
            {
                for (int i = 0; i < animation.animations[this.selectedAnimation].KeyFrames.Length; i++)
                {
                    // Make sure this keyframe is for the root node.
                    KeyFrameDescriptor keyFrameDesc = animation.animations[this.selectedAnimation].KeyFrames[i];
                    if (keyFrameDesc.JointIndex != 255)
                        continue;

                    // Check the key frame usage and handle accordingly.
                    switch (keyFrameDesc.Usage)
                    {
                        case 3:
                            {
                                // TODO: Root joint rotation?
                                break;
                            }
                        case 4:
                            {
                                // Root joint translation.
                                this.baseTranslation = InterpolateKeyFrame(animation, this.selectedAnimation, i,
                                    this.animationCurrentFrame, this.animationFrameCount, this.baseTranslation);
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException(string.Format("Key frame usage {0} not currently supported for root nodes!", keyFrameDesc.Usage));
                            }
                    }
                }
            }

            // Loop through all the animated joints and update animation data for the current frame.
            for (int i = 0; i < this.animatedJoints.Length; i++)
            {
                if (animation.animations[this.selectedAnimation].KeyFrames != null)
                {
                    // Loop through all of the keyframe descriptors and process any for this joint number.
                    for (int x = 0; x < animation.animations[this.selectedAnimation].KeyFrames.Length; x++)
                    {
                        // Get the key frame descriptor and check if it corresponds to the current joint number.
                        KeyFrameDescriptor keyFrameDesc = animation.animations[this.selectedAnimation].KeyFrames[x];
                        if (keyFrameDesc.JointIndex != this.animatedJoints[i].JointNumber)
                            continue;

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
                                    this.animatedJoints[keyFrameDesc.JointIndex].InterpolatedRotation = InterpolateKeyFrame(animation, this.selectedAnimation, x,
                                        this.animationCurrentFrame, this.animationFrameCount, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                                    break;
                                }
                            case 1:
                                {
                                    // Joint translation.
                                    this.animatedJoints[keyFrameDesc.JointIndex].InterpolatedTranslation = InterpolateKeyFrame(animation, this.selectedAnimation, x,
                                        this.animationCurrentFrame, this.animationFrameCount, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
                                    break;
                                }
                            case 2:
                                {
                                    // Joint scale.
                                    this.animatedJoints[keyFrameDesc.JointIndex].InterpolatedScale = InterpolateKeyFrame(animation, this.selectedAnimation, x,
                                        this.animationCurrentFrame, this.animationFrameCount, new Vector4(1.0f));
                                    break;
                                }
                            case 3:
                                {
                                    break;
                                }
                            case 4:
                                {
                                    // Root joint translation.
                                    this.baseTranslation = InterpolateKeyFrame(animation, this.selectedAnimation, x,
                                        this.animationCurrentFrame, this.animationFrameCount, this.baseTranslation);
                                    break;
                                }
                            default:
                                {
                                    throw new NotSupportedException(string.Format("Key frame usage {0} not currently supported!", keyFrameDesc.Usage));
                                }
                        }
                    }
                }

                // Setup root node animation matrix.
                Matrix rootNodeMatrix = Matrix.RotationQuaternion(this.modelRotation.ToQuaternion());
                rootNodeMatrix.Column4 = new Vector4(this.modelPosition.ToVector3(), 1.0f);

                // Loop through all joints and compute the final animation vectors.
                Vector4 parentJointPosition = new Vector4(0.0f);

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
                                    jointMatrix.Column4 = joint.InterpolatedTranslation; //
                                    parentMatrix *= jointMatrix;
                                }
                            }

                            // Transform the root joint's position by the to-parent matrix.
                            parentJointPosition = Vector4.Transform(parentMatrix.Column4, rootNodeMatrix);
                            break;
                        }
                    case 20:
                        {
                            // Transform the interpolated translation by the root node matrix.
                            Vector3 newTranslation = Vector3.TransformNormal(this.animatedJoints[i].InterpolatedTranslation.ToVector3(), rootNodeMatrix);
                            newTranslation = (newTranslation + rootNodeMatrix.Column4.ToVector3()) - parentJointPosition.ToVector3();

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
                    GetJointPosition(this.animatedJoints[i].JointNumber), this.animatedJoints[i].Rotation.ToQuaternion(), this.animatedJoints[i].Translation.ToVector3());

                if (i == 2)
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format("Trans Y: {0}", this.animatedJoints[i].Translation.Y));
                }

                // Update the joint bounding sphere.
                this.jointBoundingSpheres[i].Position = GetJointPosition(this.animatedJoints[i].JointNumber);
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

        /// <summary>
        /// Sets the active animation to be played on the model
        /// </summary>
        /// <param name="animation">Animation to play on the model</param>
        public void SetActiveAnimation(rMotionList animation)
        {
            // Set the animation data.
            this.activeAnimation = animation;
            if (this.activeAnimation != null)
            {
                // Set the selected animation.
                this.selectedAnimation = 0;
                this.animationFrameRate = 15.0f;
                this.animationTimePerFrame = 1.0f / this.animationFrameRate;
                this.animationTotalTime = this.animationTimePerFrame * animation.animations[0].FrameCount;
                this.animationCurrentTime = 0.0f;
                this.animationFrameCount = animation.animations[this.selectedAnimation].FrameCount;
                this.animationCurrentFrame = 0.0f;
            }
            else
            {
                // Reset animation counters.
            }
        }

        #endregion

        #region IRenderable

        public override bool InitializeGraphics(RenderManager manager)
        {
            // Create our vertex and index buffers from the model data.
            this.primaryVertexBuffer = Buffer.Create<byte>(manager.Device, BindFlags.VertexBuffer, this.vertexData1);
            if (this.vertexData2 != null)
                this.secondaryVertexBuffer = Buffer.Create<byte>(manager.Device, BindFlags.VertexBuffer, this.vertexData2);
            this.indexBuffer = Buffer.Create<ushort>(manager.Device, BindFlags.IndexBuffer, this.indexData);

            // If this is a sky model we need to fix up the textures.
            if (this.FileName.EndsWith("sky.rModel", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Allocate a new texture array.
                this.header.NumberOfTextures = 3;
                this.textureFileNames = new string[3];

                // Get the file name for this model without the extension.
                string fileName = this.FileName.Substring(0, this.FileName.LastIndexOf("."));

                // Setup the texture file names array.
                this.textureFileNames[0] = fileName + "_IM-00_d";
                this.textureFileNames[1] = fileName + "_IM-00_s";
                this.textureFileNames[2] = fileName + "_XM-00_n";

                // Fixup the material to use the day bitmap.
                this.materials[0].BaseMapTexture = 1;
            }

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
                    this.dxTextures[i] = new Texture2D(manager.Device, desc);
                    manager.Device.ImmediateContext.UpdateSubresource(this.gameTextures[i].SubResources[0], this.dxTextures[i]);

                    // Create the shader resource that will be use this texture.
                    this.shaderResources[i] = new ShaderResourceView(manager.Device, this.dxTextures[i]);
                }
            }

            // Load all the geometry shaders that we might need.
            this.shaders = new Shader[3];
            this.shaders[0] = manager.ShaderCollection.GetShader(ShaderType.SkinnedRigid8W);
            this.shaders[1] = manager.ShaderCollection.GetShader(ShaderType.SkinnedRigid4W);
            this.shaders[2] = manager.ShaderCollection.GetShader(ShaderType.Game_LevelGeometry1);

            // Setup the blend state for image transparency.
            BlendStateDescription blendDesc = new BlendStateDescription();
            blendDesc.AlphaToCoverageEnable = false;
            blendDesc.IndependentBlendEnable = false;
            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            this.transparencyBlendState = new BlendState(manager.Device, blendDesc);

            // Allocate the primtive bounding box array and initialize each one.
            this.primitiveBoxes = new BoundingBox[this.primitives.Length];
            for (int i = 0; i < this.primitives.Length; i++)
            {
                // Initialize the bounding box.
                this.primitiveBoxes[i] = new BoundingBox(this.primitives[i].BoundingBoxMin, this.primitives[i].BoundingBoxMax, new Color4(0xFFFF0000));
                if (this.primitiveBoxes[i].InitializeGraphics(manager) == false)
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
                this.animatedJoints[i].SRTMatrix = Matrix.Identity;// this.jointTranslations[i].SRTMatrix;

                // Create the bounding sphere for the current joint.
                float jointRadius = this.joints[i].Length > 0.0f ? this.joints[i].Length : 3.0f;
                this.jointBoundingSpheres[i] = new BoundingSphere(jointPosition, this.animatedJoints[i].Rotation, jointRadius, new Color4(0xFF00FF00));
                if (this.jointBoundingSpheres[i].InitializeGraphics(manager) == false)
                {
                    // Failed to initialize graphics for bounding sphere.
                    return false;
                }
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
                this.boneMapMatrix = new Texture2D(manager.Device, desc);

                // Create the shader resource view that will bind this texture.
                this.boneMapMatrixShaderView = new ShaderResourceView(manager.Device, this.boneMapMatrix);
            }

            // Loop and build the list of primitives by group id.
            this.visiblePrimitives = new bool[this.primitives.Length];
            for (int i = 0; i < this.primitives.Length; i++)
            {
                // Set the primitive to visible.
                this.visiblePrimitives[i] = true;

                // Check if the key for the group id exists.
                if (this.primitivesByGroup.ContainsKey(this.primitives[i].GroupID) == false)
                {
                    // Create a new list for this group id.
                    this.primitivesByGroup.Add(this.primitives[i].GroupID, new SortedSet<int>());
                }

                // Append the primtive id list.
                this.primitivesByGroup[this.primitives[i].GroupID].Add(i);
            }

            // Initialize the visible group id list.
            this.visibleGroupIds = new bool[this.primitivesByGroup.Keys.Count];
            for (int i = 0; i < this.visibleGroupIds.Length; i++)
                this.visibleGroupIds[i] = true;

            // Sort the group id keys.
            this.primtiveGroupIds = this.primitivesByGroup.Keys.ToArray();
            Array.Sort(this.primtiveGroupIds);

            // Successfully initialized.
            return true;
        }

        public override bool DrawFrame(RenderManager manager)
        {
            // Check if there is an animation loaded and if so update.
            if (this.activeAnimation != null)
            {
                // Get the input manager and check for animation button input.
                if (manager.InputManager.ButtonPressed(InputAction.NextAnimation) == true || manager.InputManager.ButtonPressed(InputAction.PreviousAnimation) == true)
                {
                    // Handle input accordingly.
                    if (manager.InputManager.ButtonPressed(InputAction.NextAnimation) == true)
                    {
                        // Increment or wrap the selected animation index.
                        if (++this.selectedAnimation >= this.activeAnimation.animations.Length)
                            this.selectedAnimation = 0;
                    }
                    else
                    {
                        // Decrement or wrap the selected animation index.
                        if (--this.selectedAnimation < 0)
                            this.selectedAnimation = this.activeAnimation.animations.Length - 1;
                    }

                    // Reset position and time counters.
                    this.animationTotalTime = this.animationTimePerFrame * this.activeAnimation.animations[this.selectedAnimation].FrameCount;
                    this.animationCurrentTime = 0.0f;
                    this.animationFrameCount = this.activeAnimation.animations[this.selectedAnimation].FrameCount;
                    this.animationCurrentFrame = 0.0f;
                }

                // Only update the animation time if we are not paused.
                if (this.animationPaused == false)
                {
                    // Update the current animation frame counter.
                    this.animationCurrentTime += manager.RenderTime.TimeDelta * this.animationPlaybackRate;
                }

                // Calculate the current frame position based on the current time.
                this.animationCurrentFrame = this.animationCurrentTime / this.animationTimePerFrame;

                // Check if we need to reset the frame counters.
                if (this.animationCurrentFrame >= this.animationFrameCount)
                {
                    // Reset the frame and time counters.
                    this.animationCurrentTime = 0.0f;
                    this.animationCurrentFrame = 0.0f; // this.animationCurrentTime / this.animationTimePerFrame;
                }
                else if (this.animationCurrentTime < 0.0f)
                {
                    // Loop the frame and time counters backwards.
                    this.animationCurrentTime = this.animationTotalTime - this.animationTimePerFrame;
                    this.animationCurrentFrame = this.animationFrameCount - 1;
                }

                // Update animation data for all joints.
                UpdateAnimationData(manager, this.activeAnimation);
            }

            // Loop through all of the joints and update the bone matrix data.
            for (int i = 0; i < this.animatedJoints.Length; i++)
            {
                // Copy the translation matrix into the bone matrix buffer.
                Matrix SRTMatrix = GetJointTransformation(this.animatedJoints[i].JointNumber);
                boneMatrixData[i * 4] = new Color4(SRTMatrix.Column1);
                boneMatrixData[i * 4 + 1] = new Color4(SRTMatrix.Column2);
                boneMatrixData[i * 4 + 2] = new Color4(SRTMatrix.Column3);
                boneMatrixData[i * 4 + 3] = new Color4(SRTMatrix.Column4);
            }

            // Update the bone matrix map texture.
            manager.Device.ImmediateContext.UpdateSubresource(this.boneMatrixData, this.boneMapMatrix, rowPitch: 16 * 4);

            // Set the primitive type.
            manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            // Set alpha blending state.
            manager.Device.ImmediateContext.OutputMerger.SetBlendState(this.transparencyBlendState, new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f));

            // Set the world transform based on the model position.
            Matrix world = Matrix.Transformation(new Vector3(0.0f), new Quaternion(0.0f), this.modelScale.ToVector3(), 
                new Vector3(0.0f), this.modelRotation.ToQuaternion(), this.modelPosition.ToVector3());
            manager.ShaderConstants.gXfViewProj = Matrix.Transpose(world * manager.Camera.ViewMatrix * manager.ProjectionMatrix);

            // Set the bounding box parameters for this model.
            manager.ShaderConstants.gXfQuantPosScale = this.header.BoundingBoxMax - this.header.BoundingBoxMin;
            manager.ShaderConstants.gXfQuantPosOffset = this.header.BoundingBoxMin;

            // Set the bone map matrix data.
            float matrixUnitSize = 1.0f / (float)NextPowerOfTwo(this.joints.Length);
            float matrixRowSize = matrixUnitSize / 4.0f;
            manager.ShaderConstants.gXfMatrixMapFactor = new Vector4(0.0f, 0.0f, matrixUnitSize, matrixRowSize);
            manager.Device.ImmediateContext.VertexShader.SetShaderResource(0, this.boneMapMatrixShaderView);

            // Update shader constants now to avoid doing it every frame for non-highlighted objects.
            manager.UpdateShaderConstants();

            // Loop through all of the primitives for the model and draw each one.
            for (int i = 0; i < this.primitives.Length; i++)
            {
                // Check if the primitive is enabled.
                if (this.primitives[i].Enabled == 0 || this.visiblePrimitives[i] == false)
                    continue;

                // Set the depth stencil to normal z-test.
                //manager.SetDepthStencil(0);

                // Set the vertex and index buffers.
                manager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(this.primaryVertexBuffer, this.primitives[i].VertexStride1, this.primitives[i].VertexStream1Offset),
                    new VertexBufferBinding(this.secondaryVertexBuffer, this.primitives[i].VertexStride2, this.primitives[i].VertexStream2Offset));
                manager.Device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

                // Check the flags on the material for this primitive to determine what shader to render with.
                int shaderFlags = (this.materials[this.primitives[i].MaterialIndex].Flags >> 27) & 7;
                switch (shaderFlags)
                {
                    case 0: this.shaders[1].DrawFrame(manager); break;
                    case 1: this.shaders[0].DrawFrame(manager); break;
                    case 2: this.shaders[2].DrawFrame(manager); break;
                }

                // Get the material for the primitive.
                Material material = this.materials[this.primitives[i].MaterialIndex];

                // Set the textures being used by the material.
                manager.Device.ImmediateContext.PixelShader.SetShaderResource(0, this.shaderResources[material.BaseMapTexture]);

                // Draw the primtive.
                manager.Device.ImmediateContext.DrawIndexed(this.primitives[i].IndexCount, this.primitives[i].StartingIndexLocation, this.primitives[i].BaseVertexLocation);

                // Check if we should highlight the primitive based on UI.
                if (this.hoveredPrimitiveIndex == i)
                {
                    // Set the depth stencil for object bleeding.
                    //manager.SetDepthStencil(1);

                    // Set the highlighting shader variables.
                    manager.ShaderConstants.gXfHighlightColor = new Vector4(1.0f, 0.0f, 0.0f, 0.5f);
                    manager.ShaderConstants.gXfHighlightingEnabled = 1;

                    // Update shader constants.
                    manager.UpdateShaderConstants();

                    // Draw the object again with no depth test and using the highlight color.
                    manager.Device.ImmediateContext.DrawIndexed(this.primitives[i].IndexCount, this.primitives[i].StartingIndexLocation, this.primitives[i].BaseVertexLocation);

                    // Turn highlighting off and update the shader constants buffer.
                    manager.ShaderConstants.gXfHighlightingEnabled = 0;
                    manager.UpdateShaderConstants();
                }
            }

            // Joint bounding speheres:
            if (this.showJointSpheres == true)
            {
                // Loop and draw bounding sphere for all the joints in the mesh.
                for (int i = 0; i < this.jointBoundingSpheres.Length; i++)
                {
                    // Draw the bounding sphere.
                    this.jointBoundingSpheres[i].DrawFrame(manager);
                }
            }

            // Primitive bounding boxes:
            if (this.showBoundingBoxes == true)
            {
                // Loop and draw the bounding boxes for all the primitives in the mesh.
                for (int i = 0; i < this.primitiveBoxes.Length; i++)
                {
                    // Draw the bounding box.
                    this.primitiveBoxes[i].DrawFrame(manager);
                }
            }

            // Done rendering.
            return true;
        }

        public override void CleanupGraphics(RenderManager manager)
        {
            // TODO:
        }

        #endregion

        #region UI Rendering

        public override void DrawObjectPropertiesUI(RenderManager manager)
        {
            // Reset hovered index.
            this.hoveredPrimitiveIndex = -1;

            // Display checkboxes for bounding spheres and boxes.
            ImGui.Checkbox("Show joint spheres", ref this.showJointSpheres);
            ImGui.Checkbox("Show bounding boxes", ref this.showBoundingBoxes);

            // Create the tab page header.
            if (ImGui.BeginTabBar("##tabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton) == true)
            {
                #region Materials

                // Materials section.
                if (ImGui.BeginTabItem("Materials") == true)
                {
                    ImGui.BeginChild("##materialsbox");

                    // Create a scrollable list for material selection.
                    ImVector2 boxSize = ImGui.GetItemRectSize();
                    ImGui.BeginChild("MaterialsSelection", new ImVector2(boxSize.X - 17, 200), true);
                    for (int i = 0; i < this.materials.Length; i++)
                    {
                        // Create a option for this material.
                        if (ImGui.Selectable("Material " + i.ToString(), this.selectedMaterialIndex == i) == true)
                        {
                            // Set the selected material index.
                            this.selectedMaterialIndex = i;
                        }
                    }
                    ImGui.EndChild();

                    // Display the material properties for the selected material.
                    ImGui.Text("Shader Technique: " + this.materials[this.selectedMaterialIndex].ShaderTechnique.ToString());

                    Tuple<string, int>[] textureInfo = new Tuple<string, int>[]
                        {
                            new Tuple<string, int>("Basemap", this.materials[this.selectedMaterialIndex].BaseMapTexture),
                            new Tuple<string, int>("Bumpmap", this.materials[this.selectedMaterialIndex].NormalMapTexture),
                            new Tuple<string, int>("Maskmap", this.materials[this.selectedMaterialIndex].MaskMapTexture),
                            new Tuple<string, int>("Lightmap", this.materials[this.selectedMaterialIndex].LightmapTexture),
                            new Tuple<string, int>("", this.materials[this.selectedMaterialIndex].TextureIndex5),
                            new Tuple<string, int>("", this.materials[this.selectedMaterialIndex].TextureIndex6),
                            new Tuple<string, int>("", this.materials[this.selectedMaterialIndex].TextureIndex7),
                            new Tuple<string, int>("", this.materials[this.selectedMaterialIndex].TextureIndex8),
                            new Tuple<string, int>("", this.materials[this.selectedMaterialIndex].TextureIndex9),
                        };

                    // Loop and setup texture previews for all texture placeholders.
                    ImGui.Separator();
                    ImGui.Columns(3, "texturecolumns", false);
                    for (int i = 0; i < textureInfo.Length; i++)
                    {
                        // If the texture is set show a preview, else show the empty placeholder texture.
                        ImGui.Text(textureInfo[i].Item1);
                        if (textureInfo[i].Item2 != 0 && this.shaderResources[textureInfo[i].Item2] != null)
                        {
                            ImGui.Image(this.shaderResources[textureInfo[i].Item2].NativePointer, new ImVector2(150, 150));
                            if (ImGui.IsItemHovered() == true)
                            {
                                // Create a tooltip window for a larger preview of the image.
                                ImGui.BeginTooltip();
                                ImGui.Image(this.shaderResources[textureInfo[i].Item2].NativePointer, new ImVector2(512, 512));
                                ImGui.EndTooltip();
                            }
                        }
                        else
                        {
                            // Use the place holder texture.
                            ImGui.Image(manager.CheckerboardTextureResource.NativePointer, new ImVector2(150, 150));
                        }
                        ImGui.NextColumn();
                    }
                    ImGui.Columns(1);
                    ImGui.Separator();

                    ImGui.InputFloat("Transparency", ref this.materials[this.selectedMaterialIndex].Transparency);
                    ImGui.InputFloat("##Unk11", ref this.materials[this.selectedMaterialIndex].Unk11);
                    ImGui.InputFloat("Fresnel Factor", ref this.materials[this.selectedMaterialIndex].FresnelFactor);
                    ImGui.InputFloat("Fresnel Bias", ref this.materials[this.selectedMaterialIndex].FresnelBias);
                    ImGui.InputFloat("Specular Power", ref this.materials[this.selectedMaterialIndex].SpecularPow);
                    ImGui.InputFloat("Envmap Power", ref this.materials[this.selectedMaterialIndex].EnvmapPower);
                    ImGui.InputFloat4("Lightmap Scale", ref this.materials[this.selectedMaterialIndex].LightMapScale);
                    ImGui.InputFloat("Detail Factor", ref this.materials[this.selectedMaterialIndex].DetailFactor);
                    ImGui.InputFloat("Detail Wrap", ref this.materials[this.selectedMaterialIndex].DetailWrap);
                    ImGui.InputFloat("##Unk22", ref this.materials[this.selectedMaterialIndex].Unk22);
                    ImGui.InputFloat("##Unk23", ref this.materials[this.selectedMaterialIndex].Unk23);
                    ImGui.InputFloat4("Transmit", ref this.materials[this.selectedMaterialIndex].Transmit);
                    ImGui.InputFloat4("Parallax", ref this.materials[this.selectedMaterialIndex].Parallax);
                    ImGui.InputFloat("##Unk32", ref this.materials[this.selectedMaterialIndex].Unk32);
                    ImGui.InputFloat("##Unk33", ref this.materials[this.selectedMaterialIndex].Unk33);
                    ImGui.InputFloat("##Unk34", ref this.materials[this.selectedMaterialIndex].Unk34);
                    ImGui.InputFloat("##Unk35", ref this.materials[this.selectedMaterialIndex].Unk35);

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                #endregion

                #region Primitives

                // Primitives section.
                if (ImGui.BeginTabItem("Primitives") == true)
                {
                    ImGui.BeginChild("##primitivesbox");

                    // Create the scrollable treeview for primtive selection.
                    ImVector2 boxSize = ImGui.GetItemRectSize();
                    ImGui.BeginChild("PrimitiveTree", new ImVector2(boxSize.X - 7, 200), true);

                    // Add primitives by group into the treeview.
                    for (int i = 0; i < this.primtiveGroupIds.Length; i++)
                    {
                        // Add a checkbox that toggles the visibility of all primitives in the group.
                        if (ImGui.Checkbox("##group" + i.ToString(), ref this.visibleGroupIds[i]) == true)
                        {
                            // Loop and change the visible state of all primitives in the group.
                            foreach (int primId in this.primitivesByGroup[this.primtiveGroupIds[i]])
                                this.visiblePrimitives[primId] = this.visibleGroupIds[i];
                        }
                        ImGui.SameLine();

                        // Create a new tree node for the group id.
                        if (ImGui.TreeNodeEx("Group " + this.primtiveGroupIds[i].ToString(), ImGuiTreeNodeFlags.DefaultOpen) == true)
                        {
                            // Loop and create tree nodes for all the primitives in this group.
                            foreach (int primId in this.primitivesByGroup[this.primtiveGroupIds[i]])
                            {
                                // Add a checkbox that toggles visibility of the primitive.
                                ImGui.Checkbox("##primitive" + primId.ToString(), ref this.visiblePrimitives[primId]);
                                ImGui.SameLine();

                                // Create tree node for primitive.
                                ImGui.AlignTextToFramePadding();
                                if (ImGui.Selectable("Primitive " + primId.ToString(), this.selectedPrimitiveIndex == primId,
                                    ImGuiSelectableFlags.None, new ImVector2(140.0f, 17.0f)) == true)
                                {
                                    // Set the selected primitive index.
                                    this.selectedPrimitiveIndex = primId;
                                }

                                // If the user hovers a primitive node highlight that primitive mesh.
                                if (ImGui.IsItemHovered() == true)
                                {
                                    // Set the hovered primitive index.
                                    this.hoveredPrimitiveIndex = primId;
                                }
                            }

                            ImGui.TreePop();
                        }
                    }
                    ImGui.EndChild();

                    // Primitive info.
                    if (this.selectedPrimitiveIndex != -1)
                    {
                        // TODO: Change to drop down and add goto button for materials
                        ImGui.InputScalarInt16("Group ID", ref this.primitives[this.selectedPrimitiveIndex].GroupID);
                        ImGui.InputScalarInt16("Material index", ref this.primitives[this.selectedPrimitiveIndex].MaterialIndex);

                        bool enabled = this.primitives[this.selectedPrimitiveIndex].Enabled != 0;
                        if (ImGui.Checkbox("Enabled", ref enabled) == true)
                            this.primitives[this.selectedPrimitiveIndex].Enabled = enabled == true ? (byte)1 : (byte)0;

                        ImGui.InputInt("Vertex count", ref this.primitives[this.selectedPrimitiveIndex].VertexCount, 0, 0, ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputInt("Starting vertex", ref this.primitives[this.selectedPrimitiveIndex].StartingVertex, 0, 0, ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputInt("Base vertex", ref this.primitives[this.selectedPrimitiveIndex].BaseVertexLocation, 0, 0, ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputInt("Index count", ref this.primitives[this.selectedPrimitiveIndex].IndexCount, 0, 0, ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputInt("Starting index", ref this.primitives[this.selectedPrimitiveIndex].StartingIndexLocation, 0, 0, ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputFloat4("Bounding box min", ref this.primitives[this.selectedPrimitiveIndex].BoundingBoxMin, "%.3f", ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputFloat4("Bounding box min", ref this.primitives[this.selectedPrimitiveIndex].BoundingBoxMax, "%.3f", ImGuiInputTextFlags.ReadOnly);
                    }

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                #endregion

                #region Joints

                // Create the joints tab.
                if (ImGui.BeginTabItem("Joints") == true)
                {
                    // Loop and print info on each joint.
                    ImGui.BeginChild("##jointsbox");
                    for (int i = 0; i < this.joints.Length; i++)
                    {
                        Vector4[] matrixAsVectors = new Vector4[4]
                        {
                            this.animatedJoints[i].SRTMatrix.Row1,
                            this.animatedJoints[i].SRTMatrix.Row2,
                            this.animatedJoints[i].SRTMatrix.Row3,
                            this.animatedJoints[i].SRTMatrix.Row4
                        };

                        // Print the joint info.
                        ImGui.BeginGroup();
                        ImGui.Text("Parent: " + this.joints[i].ParentIndex.ToString());
                        ImGui.Text("Index: " + this.joints[i].Index.ToString());
                        ImGui.Text("Radius: " + this.joints[i].Length.ToString());
                        ImGui.InputFloat3("Offset", ref this.joints[i].Offset, "%.3f", ImGuiInputTextFlags.ReadOnly);
                        ImGui.InputFloat4("SRT Matrix", ref matrixAsVectors[0]);
                        ImGui.InputFloat4("", ref matrixAsVectors[1]);
                        ImGui.InputFloat4("", ref matrixAsVectors[2]);
                        ImGui.InputFloat4("", ref matrixAsVectors[3]);
                        ImGui.Separator();
                        ImGui.EndGroup();

                        // When the user hovers over the group change the color of the bounding sphere.
                        if (ImGui.IsItemHovered() == true)
                        {
                            // Change sphere color to red.
                            this.jointBoundingSpheres[i].Color = new Color4(0xFF0000FF);
                        }
                        else
                        {
                            // Normal color green.
                            this.jointBoundingSpheres[i].Color = new Color4(0xFF00FF00);
                        }
                    }
                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }

                #endregion

                #region Animation

                // Only display the animation tab if the model has joints that can be animated.
                if (this.joints.Length > 0)
                {
                    // Animation section.
                    if (ImGui.BeginTabItem("Animation") == true)
                    {
                        // Setup the animation file selection control.
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("Animation file: " + (this.activeAnimation != null ? this.activeAnimation.FileName : ""));
                        ImGui.SameLine();
                        if (ImGui.Button("...") == true)
                        {
                            // Create the file select dialog.
                            this.fileSelectDialog = new ImGuiResourceSelectDialog("Select animation", ResourceType.rMotionList);
                            this.fileSelectDialog.OnResourceSelected += new ImGuiResourceSelectDialog.OnResourceSelectedHandler((DatumIndex datum, out string errorMsg) =>
                            {
                                // Satisfy the compiler.
                                errorMsg = null;

                                // Please forgive me for I have BLOCKED ON THE UI THREAD.

                                // Parse the animation file from the archive, and make sure it has the same number of joints.
                                rMotionList newAnimation = ArchiveCollection.Instance.GetFileAsResource<rMotionList>(datum);
                                AnimationDescriptor animDesc = newAnimation.animations.First(desc => desc.JointCount > 0);
                                if (animDesc.JointCount != this.joints.Length)
                                {
                                    // Animation has incorrect joint count, display an error to the user.
                                    errorMsg = "The selected animation file does not have the same number of joints as the model!";
                                    return false;
                                }

                                // Set the animation file as the active animation to play.
                                SetActiveAnimation(newAnimation);
                                return true;
                            });

                            // Display the dialog.
                            this.fileSelectDialog.ShowDialog();
                        }

                        // Draw the file select dialog if it is being used.
                        if (this.fileSelectDialog != null)
                        {
                            if (this.fileSelectDialog.DrawDialog() == true)
                            {
                                // Destroy the file select dialog.
                                this.fileSelectDialog = null;
                            }
                        }

                        // Display animation properties is there is a selected animation.
                        if (this.activeAnimation != null)
                        {
                            // Create a combobox for the selected animation.
                            if (ImGui.BeginCombo("Animation", "Animation " + this.selectedAnimation.ToString()) == true)
                            {
                                // Add option entries for the animations.
                                for (int i = 0; i < this.activeAnimation.animations.Length; i++)
                                {
                                    // If the animation has 0 frames don't display it as an option.
                                    if (this.activeAnimation.animations[i].FrameCount == 0)
                                        continue;

                                    bool isSelected = this.selectedAnimation == i;
                                    if (ImGui.Selectable("Animation " + i.ToString(), isSelected) == true)
                                        SetActiveAnimationIndex(i);

                                    // Set focus on the selected item when first opening.
                                    if (isSelected == true)
                                        ImGui.SetItemDefaultFocus();
                                }
                                ImGui.EndCombo();
                            }

                            // Add buttons for next/previous animation.
                            ImGui.SameLine();
                            if (ImGui.ArrowButton("previous_anim", ImGuiDir.Left) == true)
                            {
                                // Loop until we find an animation with a non-zero frame count.
                                do
                                {
                                    // Decrement or wrap the selected animation index.
                                    if (--this.selectedAnimation < 0)
                                        this.selectedAnimation = this.activeAnimation.animations.Length - 1;
                                }
                                while (this.activeAnimation.animations[this.selectedAnimation].FrameCount == 0);

                                SetActiveAnimationIndex(this.selectedAnimation);
                            }

                            ImGui.SameLine();
                            if (ImGui.ArrowButton("next_anim", ImGuiDir.Right) == true)
                            {
                                // Loop until we find an animation with a non-zero frame count.
                                do
                                {
                                    // Increment or wrap the selected animation index.
                                    if (++this.selectedAnimation >= this.activeAnimation.animations.Length)
                                        this.selectedAnimation = 0;
                                }
                                while (this.activeAnimation.animations[this.selectedAnimation].FrameCount == 0);

                                SetActiveAnimationIndex(this.selectedAnimation);
                            }

                            // Animation playback info.
                            ImGui.Separator();
                            ImGui.Text("Frame count: " + this.animationFrameCount.ToString());
                            ImGui.Text("Total time: " + this.animationTotalTime.ToString() + " sec");
                            if (ImGui.InputFloat("Playback speed", ref this.animationPlaybackRate, 0.25f) == true)
                            {
                                // Make sure the playback rate never goes below 0.
                                if (this.animationPlaybackRate < 0.0f)
                                    this.animationPlaybackRate = 0.0f;
                            }
                            if (ImGui.SliderFloat("", ref this.animationCurrentFrame, 0.0f, this.animationFrameCount) == true)
                                this.animationCurrentTime = this.animationCurrentFrame * this.animationTimePerFrame;

                            // Add playback buttons.
                            if (ImGui.ArrowButton("prev_frame", ImGuiDir.Left) == true)
                            {
                                this.animationCurrentTime -= this.animationTimePerFrame;
                            }

                            ImGui.SameLine();
                            if (ImGui.Button("[Play]") == true)
                                this.animationPaused = false;

                            ImGui.SameLine();
                            if (ImGui.Button("[Pause]") == true)
                                this.animationPaused = true;

                            ImGui.SameLine();
                            if (ImGui.Button("[Stop]") == true)
                            {
                                this.animationPaused = true;
                                SetActiveAnimationIndex(this.selectedAnimation);
                            }

                            ImGui.SameLine();
                            if (ImGui.ArrowButton("next_frame", ImGuiDir.Right) == true)
                            {
                                this.animationCurrentTime += this.animationTimePerFrame;
                            }
                        }

                        ImGui.EndTabItem();
                    }
                }

                #endregion

                ImGui.EndTabBar();
            }
        }

        #endregion

        #region Utilities

        public void SetActiveAnimationIndex(int index)
        {
            // If there is no active animation bail out.
            if (this.activeAnimation == null)
                return;

            // Set the selected animation index and reset the animation timers.
            this.selectedAnimation = index;
            this.animationTotalTime = this.animationTimePerFrame * this.activeAnimation.animations[this.selectedAnimation].FrameCount;
            this.animationCurrentTime = 0.0f;
            this.animationFrameCount = this.activeAnimation.animations[this.selectedAnimation].FrameCount;
            this.animationCurrentFrame = 0.0f;
        }

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

        #endregion
    }
}
