using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
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
        /* 0x58 */
        /* 0x5C */
        /* 0x60 */ // vec3 bounding box sphere position?
        /* 0x6C */ // float bounding box sphere radius?
        /* 0x70 */ public Vector4 BoundingBoxMin;
        /* 0x80 */ public Vector4 BoundingBoxMax;
        /* 0x90 */
        /* 0x94 */ // used by something in code
        /* 0x98 */
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
    public struct JointTranslation
    {
        /* 0x00 */ public Vector4[] Translation;
    }

    // sizeof = 0x40
    public struct JointData3
    {
        /* 0x00 */ public Vector4[] Unknown;
    }

    // sizeof = 0xD0
    public struct Material
    {
        /* 0x00 */ public byte Flags;
        /* 0x01 */ public byte Unk1;
        /* 0x02 */ public byte Unk2;
        /* 0x03 */ public byte Unk3;
	    /* 0x04 */ public int Unk4;
	    /* 0x08 */ public int Unk9;
        /* 0x0C */ public int Unk5;
        /* 0x10 */ public int Unk6;
        /* 0x14 */ public int Unk7;
        /* 0x18 */ public int Unk8;
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
	
	    // a bunch of floats
    }

    // sizeof = 0x50
    public struct Primitive
    {
        /* 0x00 */ public short Unk1; // joint number mb?
	    /* 0x02 */ public short	MaterialIndex;
	    /* 0x04 */ public byte Enabled;
	    /* 0x05 */ public byte Unk3; // LOD?
	    /* 0x06 */ public byte Unk11;
        /* 0x07 */ public byte Unk12;
	    /* 0x08 */ public byte VertexStride1;
	    /* 0x09 */ public byte VertexStride2;
        /* 0x0A */ public byte Unk13;
	    /* 0x0B */ // padding
        /* 0x0C */ public int VertexCount;
        /* 0x10 */ public int StartingVertex;
	    /* 0x14 */ public int VertexStream1Offset;      // Passed to CDeviceContext::IASetVertexBuffers
	    /* 0x18 */ public int VertexStream2Offset;	    // Passed to CDeviceContext::IASetVertexBuffers
	    /* 0x1C */ public int StartingIndexLocation;    // Passed to CDeviceContext::DrawIndexed
	    /* 0x20 */ public int IndexCount;               // Passed to CDeviceContext::DrawIndexed
	    /* 0x24 */ public int BaseVertexLocation;       // Passed to CDeviceContext::DrawIndexed
	    /* 0x28 */ // padding to align vectors
	    /* 0x30 */ public Vector4 Unk9;
	    /* 0x40 */ public Vector4 Unk10;
    }

    [GameResourceParser(ResourceType.rModel)]
    public class rModel : GameResource
    {
        public rModelHeader header;

        // Joint data is broken into 3 parts per joint.
        public Joint[] joints;
        public JointTranslation[] jointTranslations;
        public JointData3[] jointData3;

        // List of texture files names.
        public string[] textureFileNames;

        // List of materials.
        public Material[] materials;

        // List of primitives.
        public Primitive[] primitives;

        // Vertex and index buffers.
        public short[] indexData;
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
            if (model.header.JointCount > 0)
            {
                // Seek to the joint data offset.
                reader.BaseStream.Position = model.header.JointDataOffset;

                // Allocate and read all of the joint meta data.
                model.joints = new Joint[model.header.JointCount];
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
                model.jointTranslations = new JointTranslation[model.header.JointCount];
                for (int i = 0; i < model.header.JointCount; i++)
                {
                    // Read the joint translation.
                    model.jointTranslations[i] = new JointTranslation();
                    model.jointTranslations[i].Translation = new Vector4[4];
                    for (int x = 0; x < 4; x++)
                        model.jointTranslations[i].Translation[x] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

                // Allocate and read all of the joint data 3.
                model.jointData3 = new JointData3[model.header.JointCount];
                for (int i = 0; i < model.header.JointCount; i++)
                {
                    // Read data.
                    model.jointData3[i] = new JointData3();
                    model.jointData3[i].Unknown = new Vector4[4];
                    for (int x = 0; x < 4; x++)
                        model.jointData3[i].Unknown[x] = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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
                    model.materials[i].Flags = reader.ReadByte();
                    model.materials[i].Unk1 = reader.ReadByte();
                    model.materials[i].Unk2 = reader.ReadByte();
                    model.materials[i].Unk3 = reader.ReadByte();
                    model.materials[i].Unk4 = reader.ReadInt32();
                    model.materials[i].Unk9 = reader.ReadInt32();
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

                    reader.BaseStream.Position += 0x68;
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
                model.primitives[i].Unk1 = reader.ReadInt16();
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
                model.primitives[i].Unk9 = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                model.primitives[i].Unk10 = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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
            model.indexData = new short[model.header.IndiceCount - 1];
            for (int i = 0; i < model.header.IndiceCount - 1; i++)
            {
                // Read the index data.
                model.indexData[i] = reader.ReadInt16();
            }

            // Close the binary reader and memory stream.
            reader.Close();
            ms.Close();

            // Return the model object.
            return model;
        }

        #region IRenderable

        public override bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Create our vertex and index buffers from the model data.
            this.primaryVertexBuffer = Buffer.Create<byte>(device, BindFlags.VertexBuffer, this.vertexData1);
            if (this.vertexData2 != null)
                this.secondaryVertexBuffer = Buffer.Create<byte>(device, BindFlags.VertexBuffer, this.vertexData2);
            this.indexBuffer = Buffer.Create<short>(device, BindFlags.IndexBuffer, this.indexData);

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
            //blendDesc.RenderTarget[0].IsBlendEnabled = true;
            //blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            //blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            //blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            //blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            //blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            //blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Maximum;
            //blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            blendDesc.RenderTarget[0].IsBlendEnabled = false;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            this.transparencyBlendState = new BlendState(device, blendDesc);

            // Successfully initialized.
            return true;
        }

        public override bool DrawFrame(IRenderManager manager, Device device)
        {
            // TODO: Update shader constants before we call the shader DrawFrame routine.

            // Set the primitive type.
            device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            // Set alpha blending state.
            device.ImmediateContext.OutputMerger.SetBlendState(this.transparencyBlendState);

            // Loop through all of the primitives for the model and draw each one.
            for (int i = 0; i < this.primitives.Length; i++)
            {
                // Check if the primitive is enabled.
                if (this.primitives[i].Enabled == 0)
                    continue;

                // Set the vertex and index buffers.
                device.ImmediateContext.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(this.primaryVertexBuffer, this.primitives[0].VertexStride1, this.primitives[i].VertexStream1Offset),
                    new VertexBufferBinding(this.secondaryVertexBuffer, this.primitives[0].VertexStride2, this.primitives[i].VertexStream2Offset));
                device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);

                // Setup the vertex shader, pixel shader, sampler states, and vertex declaration.
                if (this.primitives[i].VertexStride1 == 28 && this.primitives[i].VertexStride2 == 12)
                {
                    // Normal mesh shader.
                    this.shaders[1].DrawFrame(manager, device);
                }
                else if (this.primitives[i].VertexStride1 == 28 && this.primitives[i].VertexStride2 == 28)
                {
                    // Level geometry shader.
                    this.shaders[2].DrawFrame(manager, device);
                }
                else if (this.primitives[i].VertexStride1 == 28 && this.primitives[i].VertexStride2 == 0)
                {
                    // TODO: I think this is the incorrect vertex declaration...
                    this.shaders[2].DrawFrame(manager, device);
                }
                else
                {
                    /*
                     * Most likely this:
                        POSITION		0	DXGI_FORMAT_R32G32B32_FLOAT		0	0	0	0
		                COLOR			0	DXGI_FORMAT_B8G8R8A8_UNORM		0	12	0	0
		                TEXCOORD		0	DXGI_FORMAT_R16G16B16A16_SNORM	0	16	0	0
		                TEXCOORD		1	DXGI_FORMAT_R8G8B8A8_UINT		0	24	0	0
		                TEXCOORD		2	DXGI_FORMAT_R8G8B8A8_UINT		0	28	0	0
                     * 
                     */
                    continue;
                }

                // Get the material for the primitive.
                Material material = this.materials[this.primitives[i].MaterialIndex];

                // Set the textures being used by the material.
                device.ImmediateContext.PixelShader.SetShaderResource(0, this.shaderResources[material.BaseMapTexture]);

                // Draw the primtive.
                device.ImmediateContext.DrawIndexed(this.primitives[i].IndexCount, this.primitives[i].StartingIndexLocation, this.primitives[i].BaseVertexLocation);
            }

            // Done rendering.
            return true;
        }

        public override void CleanupGraphics(IRenderManager manager, Device device)
        {
            // TODO:
        }

        #endregion
    }
}
