using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.Utilities;
using NvTriStripDotNet;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Importers
{
    public struct DRModelHeader
    {
        /* 0x00 */ public int Version;
        /* 0x04 */ public int MaterialCount;
        /* 0x08 */ public int FaceCount;
        /* 0x0C */ public int VertexCount;
    }

    public struct DRModelMaterial
    {
        public string Name;
        public string BaseMap;
        public string BaseMapRemapPath;
        public string NormalMap;
        public string NormalMapRemapPath;
        public string MetalicMap;
        public string MetalicMapRemapPath;
    }

    public struct DRModelFace
    {
        /* 0x00 */ public int MaterialIndex;
        /* 0x04 */ public int Vertex0;
        /* 0x08 */ public int Vertex1;
        /* 0x0C */ public int Vertex2;
    }

    public struct DRModelVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector2 Texcoord0;
        public Vector2 Texcoord1;
        public Vector2 Texcoord2;
        public Vector2 Texcoord3;
        public Vector4 BoneWeights;
        public int[] BoneIndices;
    }

    public struct DRModelPrimitive
    {
        public int GroupID;
        public int MaterialIndex;

        public List<DRModelVertex> Vertices;
        public List<ushort> Indices;
    }

    public class ModelImporter
    {
        /// <summary>
        /// File path of the dead rising model file to be imported
        /// </summary>
        public string SourceFile { get; private set; }
        /// <summary>
        /// New file path used for the archive
        /// </summary>
        public string FilePath { get; set; }
        public DRModelHeader Header;
        public DRModelMaterial[] Materials;
        public DRModelFace[] Faces;
        public DRModelVertex[] Vertices;

        public ModelImporter(string sourceFilePath)
        {
            // Initialize fields.
            this.SourceFile = sourceFilePath;
        }

        public bool ReadImportFile()
        {
            // Open the input file for reading.
            using (BinaryReader reader = new BinaryReader(new FileStream(this.SourceFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Parse the header.
                this.Header = new DRModelHeader();
                this.Header.Version = reader.ReadInt32();
                this.Header.MaterialCount = reader.ReadInt32();
                this.Header.FaceCount = reader.ReadInt32();
                this.Header.VertexCount = reader.ReadInt32();

                // Check the version is supported.
                if (this.Header.Version != 1)
                    return false;

                // Parse materials:
                this.Materials = new DRModelMaterial[this.Header.MaterialCount];
                for (int i = 0; i < this.Header.MaterialCount; i++)
                {
                    this.Materials[i] = new DRModelMaterial();
                    this.Materials[i].Name = reader.ReadNullTerminatedString();
                    this.Materials[i].BaseMap = reader.ReadNullTerminatedString();
                    this.Materials[i].NormalMap = reader.ReadNullTerminatedString();
                }

                // Parse faces:
                this.Faces = new DRModelFace[this.Header.FaceCount];
                for (int i = 0; i < this.Header.FaceCount; i++)
                {
                    this.Faces[i] = new DRModelFace();
                    this.Faces[i].MaterialIndex = reader.ReadInt32();
                    this.Faces[i].Vertex0 = reader.ReadInt32();
                    this.Faces[i].Vertex1 = reader.ReadInt32();
                    this.Faces[i].Vertex2 = reader.ReadInt32();
                }

                // Parse vertices:
                this.Vertices = new DRModelVertex[this.Header.VertexCount];
                for (int i = 0; i < this.Header.VertexCount; i++)
                {
                    this.Vertices[i] = new DRModelVertex();
                    this.Vertices[i].Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].Tangent = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].Texcoord0 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].Texcoord1 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].Texcoord2 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].Texcoord3 = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].BoneWeights = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    this.Vertices[i].BoneIndices = new int[4] { reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32() };
                }
            }

            return true;
        }

        public bool ImportModel(Archive.Archive archive)
        {
            List<rTexture> textures = new List<rTexture>();
            Dictionary<string, int> textureLookupDictionary = new Dictionary<string, int>();

            bool ImportTexture(string filePath, string remappedPath, out int textureIndex)
            {
                textureIndex = 0;

                // If the file path is empty return success.
                if (string.IsNullOrEmpty(filePath) == true)
                    return true;

                // Check if the texture has already been imported.
                if (textureLookupDictionary.ContainsKey(remappedPath) == false)
                {
                    // Parse the dds image.
                    DDSImage ddsImage = DDSImage.FromFile(filePath);
                    if (ddsImage == null)
                        return false;

                    // Remapped path cannot be more than 64 characters.
                    Debug.Assert(remappedPath.Length <= 64);

                    // Convert the dds image to an rTexture file.
                    rTexture texture = rTexture.FromDDSImage(ddsImage, remappedPath, new DatumIndex(DatumIndex.Unassigned), ResourceType.rTexture, false);

                    // Add the texture to the archive.
                    if (archive.AddFile(remappedPath, texture.ToBuffer(), out DatumIndex textureDatum) == false)
                        return false;

                    // Assign the datum to the texture instance.
                    texture.Datum = textureDatum;

                    // Add the texture to the materials list.
                    textureLookupDictionary.Add(remappedPath, textures.Count);
                    textures.Add(texture);
                }

                // Return the texture index.
                textureIndex = textureLookupDictionary[remappedPath] + 1;
                return true;
            }

            // Loop and import all the material textures.
            Material[] newMaterials = new Material[this.Materials.Length];
            for (int i = 0; i < this.Materials.Length; i++)
            {
                // Initialize the material with default values.
                newMaterials[i] = new Material();
                newMaterials[i].ShaderTechnique = ShaderTechnique.tXfMaterialStandard;

                // TODO: Set shader type, for now just use 4 weight skinned.
                //newMaterials[i].Flags |= (1 << 27) | 0x41;

                // Disable alpha clipping.
                //newMaterials[i].Flags |= 0x40;
                //newMaterials[i].Flags |= 0x28001;
                newMaterials[i].Flags = 0x100C1;

                newMaterials[i].Unk4 = 0x44C3;
                newMaterials[i].Unk5 = 143;
                unchecked { newMaterials[i].Unk6 = (int)0xAB4E2860; }
                newMaterials[i].Unk7 = 197;
                newMaterials[i].Unk8 = -1;

                newMaterials[i].Transparency = 1f;

                //newMaterials[i].FresnelFactor = 0.08f;
                //newMaterials[i].FresnelBias = 0.05f;
                //newMaterials[i].SpecularPow = 16;
                //newMaterials[i].EnvmapPower = 3f;
                newMaterials[i].FresnelFactor = 0.03f;
                newMaterials[i].FresnelBias = 0.5f;
                newMaterials[i].SpecularPow = 16;
                newMaterials[i].EnvmapPower = 1f;
                newMaterials[i].LightMapScale = new Vector4(1f, 1f, 1f, 0f);
                newMaterials[i].DetailFactor = 0.5f;
                newMaterials[i].DetailWrap = 10f;
                newMaterials[i].Unk22 = 1;
                newMaterials[i].Transmit = new Vector4(1f, 1f, 1f, 0f);
                newMaterials[i].Parallax = new Vector4(0f, 0f, 1f, 0f);

                // Import textures if assigned.
                ImportTexture(this.Materials[i].BaseMap, this.Materials[i].BaseMapRemapPath, out newMaterials[i].BaseMapTexture);
                ImportTexture(this.Materials[i].NormalMap, this.Materials[i].NormalMapRemapPath, out newMaterials[i].NormalMapTexture);
                ImportTexture(this.Materials[i].MetalicMap, this.Materials[i].MetalicMapRemapPath, out newMaterials[i].MaskMapTexture);
            }

            // Initialize primitive list.
            DRModelPrimitive[] primitives = new DRModelPrimitive[this.Header.MaterialCount];
            for (int i = 0; i < primitives.Length; i++)
            {
                primitives[i] = new DRModelPrimitive();
                primitives[i].GroupID = 0;
                primitives[i].MaterialIndex = i;
                primitives[i].Vertices = new List<DRModelVertex>();
                primitives[i].Indices = new List<ushort>();
            }

            // Loop through all the faces and create primitives based on material type.
            for (int i = 0; i < this.Faces.Length; i++)
            {
                // If the face has no material skip it.
                if (this.Faces[i].MaterialIndex == -1)
                    continue;

                // Setup indices for the triangle.
                int primIndex = this.Faces[i].MaterialIndex;
                int vertexIndex = primitives[primIndex].Vertices.Count;
                primitives[primIndex].Indices.Add((ushort)(vertexIndex + 0));
                primitives[primIndex].Indices.Add((ushort)(vertexIndex + 1));
                primitives[primIndex].Indices.Add((ushort)(vertexIndex + 2));

                // Add vertices.
                primitives[primIndex].Vertices.Add(this.Vertices[this.Faces[i].Vertex0]);
                primitives[primIndex].Vertices.Add(this.Vertices[this.Faces[i].Vertex1]);
                primitives[primIndex].Vertices.Add(this.Vertices[this.Faces[i].Vertex2]);
            }

            // Create a memory stream to write the model data to.
            MemoryStream modelStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(modelStream);

            // Create memory streams for the vertex data streams.
            MemoryStream vertexDataStream1 = new MemoryStream();
            MemoryStream vertexDataStream2 = new MemoryStream();

            // Initialize the header with info we currently have.
            rModelHeader header = new rModelHeader();
            header.Magic = rModelHeader.kMagic;
            header.Version = rModelHeader.kVersion;
            header.SubVersion = rModelHeader.kSubVersion;
            header.JointCount = 18;
            header.PrimitiveCount = (short)primitives.Length;
            header.MaterialCount = (short)this.Materials.Length;
            header.PolygonCount = this.Faces.Length;
            header.NumberOfTextures = textures.Count;
            header.MidDist = 1000;
            header.LowDist = 3000;
            header.LightGroup = 2;

            // Calculate the min/max extents of the mesh.
            header.BoundingBoxMin = new Vector4(VectorMinimums(this.Vertices.Select(v => v.Position).ToArray()), 0f);
            header.BoundingBoxMax = new Vector4(VectorMaximums(this.Vertices.Select(v => v.Position).ToArray()), 0f);

            // Write an empty header for now.
            writer.Write(new byte[rModelHeader.kSizeOf]);
            writer.AlignToBoundary(16, 0xCD);

            // Write out joint data.
            header.JointDataOffset = (int)writer.BaseStream.Position;
            for (int i = 0; i < header.JointCount; i++)
            {
                writer.Write((byte)0);
                writer.Write((byte)255);
                writer.Write(new byte[6]);
                writer.Write(0.0f);
                writer.Write(0.0f);
                writer.Write(0.0f);
                writer.Write(0.0f);
            }

            for (int i = 0; i < header.JointCount; i++)
                writer.Write(Matrix.Identity);

            for (int i = 0; i < header.JointCount; i++)
                writer.Write(Matrix.Identity);

            writer.AlignToBoundary(16, 0xCD);

            // Loop and write texture file paths.
            header.TextureFilesOffset = (int)writer.BaseStream.Position;
            for (int i = 0; i < textures.Count; i++)
            {
                string textureName = textures[i].FileName.Substring(0, textures[i].FileName.LastIndexOf('.'));
                writer.Write(textureName.ToCharArray());
                writer.Write(new byte[64 - textureName.Length]);
            }

            // Loop and write out materials.
            for (int i = 0; i < newMaterials.Length; i++)
            {
                writer.Write(newMaterials[i].Flags);
                writer.Write(newMaterials[i].Unk4);
                writer.Write((int)newMaterials[i].ShaderTechnique);
                writer.Write(newMaterials[i].Unk5);
                writer.Write(newMaterials[i].Unk6);
                writer.Write(newMaterials[i].Unk7);
                writer.Write(newMaterials[i].Unk8);
                writer.Write(0);
                writer.Write(newMaterials[i].BaseMapTexture);
                writer.Write(0);
                writer.Write(newMaterials[i].NormalMapTexture);
                writer.Write(0);
                writer.Write(newMaterials[i].MaskMapTexture);
                writer.Write(0);
                writer.Write(newMaterials[i].LightmapTexture);
                writer.Write(0);
                writer.Write(newMaterials[i].TextureIndex5);
                writer.Write(0);
                writer.Write(newMaterials[i].TextureIndex6);
                writer.Write(0);
                writer.Write(newMaterials[i].TextureIndex7);
                writer.Write(0);
                writer.Write(newMaterials[i].TextureIndex8);
                writer.Write(0);
                writer.Write(newMaterials[i].TextureIndex9);
                writer.Write(0);
                writer.Write(newMaterials[i].Transparency);
                writer.Write(newMaterials[i].Unk11);
                writer.Write(newMaterials[i].FresnelFactor);
                writer.Write(newMaterials[i].FresnelBias);
                writer.Write(newMaterials[i].SpecularPow);
                writer.Write(newMaterials[i].EnvmapPower);
                writer.Write(newMaterials[i].LightMapScale);
                writer.Write(newMaterials[i].DetailFactor);
                writer.Write(newMaterials[i].DetailWrap);
                writer.Write(newMaterials[i].Unk22);
                writer.Write(newMaterials[i].Unk23);
                writer.Write(newMaterials[i].Transmit);
                writer.Write(newMaterials[i].Parallax);
                writer.Write(newMaterials[i].Unk32);
                writer.Write(newMaterials[i].Unk33);
                writer.Write(newMaterials[i].Unk34);
                writer.Write(newMaterials[i].Unk35);
            }
            writer.AlignToBoundary(16, 0xCD);

            // Loop and stripify all the meshes first.
            for (int i = 0; i < primitives.Length; i++)
            {
                // Stripify the mesh.
                NvStripifier stripper = new NvStripifier();
                if (stripper.GenerateStrips(primitives[i].Indices.ToArray(), out PrimitiveGroup[] primGroups, true) == false)
                {
                    throw new Exception($"Failed to stripify mesh for primitive {i}");
                }

                if (primGroups.Length != 1 || primGroups[0].Type != PrimitiveType.TriangleStrip)
                {
                    throw new Exception("Triangle stripify produced non-supported results");
                }

                primitives[i].Indices.Clear();
                primitives[i].Indices.AddRange(primGroups[0].Indices);
            }

            // Loop and write primitive data.
            header.PrimitiveDataOffset = (int)writer.BaseStream.Position;
            for (int i = 0; i < primitives.Length; i++)
            {
                // Add a degenerate triangle to break off the strip.
                primitives[i].Indices.Add(primitives[i].Indices.Last());
                if (i == primitives.Length - 1)
                    primitives[i].Indices.Add(primitives[i].Indices.Last());
                else
                    primitives[i].Indices.Add(primitives[i].Indices.First());

                // Determine vertex format and stride.
                ShaderTechnique shaderTech = newMaterials[primitives[i].MaterialIndex].ShaderTechnique;
                int vertexType = (newMaterials[primitives[i].MaterialIndex].Flags >> 27) & 7;

                int vertexStride1 = 0, vertexStride2 = 0;
                switch (vertexType)
                {
                    case 0:
                        {
                            vertexStride1 = 28;
                            vertexStride2 = 12;
                            break;
                        }
                    case 1:
                        {
                            vertexStride1 = 36;
                            vertexStride2 = 12;
                            break;
                        }
                    default:
                        {
                            throw new Exception($"Unsupported vertex type '{vertexType}'");
                        }
                }

                // Calculate bounding box extents for the primitive.
                Vector4 primMinExtents = new Vector4(VectorMinimums(primitives[i].Vertices.Select(v => v.Position).ToArray()), 0f);
                Vector4 primMaxExtents = new Vector4(VectorMaximums(primitives[i].Vertices.Select(v => v.Position).ToArray()), 0f);

                // Write primitive info.
                writer.Write((short)primitives[i].GroupID);
                writer.Write((short)primitives[i].MaterialIndex);
                writer.Write((byte)1);
                writer.Write((byte)15);
                writer.Write((byte)0);
                writer.Write((byte)1);      // mb 1?
                writer.Write((byte)vertexStride1);
                writer.Write((byte)vertexStride2);
                writer.Write((byte)1);
                writer.Write((byte)0);
                writer.Write(primitives[i].Vertices.Count);
                writer.Write(0);
                writer.Write((int)vertexDataStream1.Position);
                writer.Write((int)vertexDataStream2.Position);
                writer.Write(header.IndiceCount);
                writer.Write(primitives[i].Indices.Count - 2);
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                writer.Write(primMinExtents);
                writer.Write(primMaxExtents);

                // Loop and write vertex data out to the vertex streams.
                for (int x = 0; x < primitives[i].Vertices.Count; x++)
                {
                    // Check the shader technique and handle accordingly.
                    switch (vertexType)
                    {
                        case 0: // Skinned 4W
                            {
                                // Position:
                                vertexDataStream1.Write(CompressVertexComponent(primitives[i].Vertices[x].Position.X, header.BoundingBoxMin.X, header.BoundingBoxMax.X), 0, 2);
                                vertexDataStream1.Write(CompressVertexComponent(primitives[i].Vertices[x].Position.Y, header.BoundingBoxMin.Y, header.BoundingBoxMax.Y), 0, 2);
                                vertexDataStream1.Write(CompressVertexComponent(primitives[i].Vertices[x].Position.Z, header.BoundingBoxMin.Z, header.BoundingBoxMax.Z), 0, 2);
                                vertexDataStream1.Write(new byte[2], 0, 2);

                                // Blend indices:
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[0]);
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[1]);
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[2]);
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[3]);

                                // Blend weights:
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[0] * 255f));
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[1] * 255f));
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[2] * 255f));
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[3] * 255f));

                                // Normal:
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Normal.X), 0, 2);
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Normal.Y), 0, 2);
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Normal.Z), 0, 2);
                                vertexDataStream1.Write(new byte[2], 0, 2);

                                // Texcoord0:
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Texcoord0.X), 0, 2);
                                vertexDataStream1.Write(CompressTexcoord(1f - primitives[i].Vertices[x].Texcoord0.Y), 0, 2);

                                // Tangent:
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Tangent.X), 0, 2);
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Tangent.Y), 0, 2);
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Tangent.Z), 0, 2);
                                vertexDataStream2.Write(new byte[2], 0, 2);

                                // Texcoord1:
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Texcoord1.X), 0, 2);
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Texcoord1.Y), 0, 2);
                                break;
                            }
                        case 1: // Skinned 8W
                            {
                                // Position:
                                vertexDataStream1.Write(CompressVertexComponent(primitives[i].Vertices[x].Position.X, header.BoundingBoxMin.X, header.BoundingBoxMax.X), 0, 2);
                                vertexDataStream1.Write(CompressVertexComponent(primitives[i].Vertices[x].Position.Y, header.BoundingBoxMin.Y, header.BoundingBoxMax.Y), 0, 2);
                                vertexDataStream1.Write(CompressVertexComponent(primitives[i].Vertices[x].Position.Z, header.BoundingBoxMin.Z, header.BoundingBoxMax.Z), 0, 2);
                                vertexDataStream1.Write(new byte[2], 0, 2);

                                // Blend indices 1:
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[0]);
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[1]);
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[2]);
                                vertexDataStream1.WriteByte((byte)primitives[i].Vertices[x].BoneIndices[3]);

                                // Blend indices 2:
                                vertexDataStream1.WriteByte(0);
                                vertexDataStream1.WriteByte(0);
                                vertexDataStream1.WriteByte(0);
                                vertexDataStream1.WriteByte(0);

                                // Blend weights 1:
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[0] * 255f));
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[1] * 255f));
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[2] * 255f));
                                vertexDataStream1.WriteByte((byte)(primitives[i].Vertices[x].BoneWeights[3] * 255f));

                                // Blend weights 2:
                                vertexDataStream1.WriteByte(0);
                                vertexDataStream1.WriteByte(0);
                                vertexDataStream1.WriteByte(0);
                                vertexDataStream1.WriteByte(0);

                                // Normal:
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Normal.X), 0, 2);
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Normal.Y), 0, 2);
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Normal.Z), 0, 2);
                                vertexDataStream1.Write(new byte[2], 0, 2);

                                // Texcoord0:
                                vertexDataStream1.Write(CompressTexcoord(primitives[i].Vertices[x].Texcoord0.X), 0, 2);
                                vertexDataStream1.Write(CompressTexcoord(1f - primitives[i].Vertices[x].Texcoord0.Y), 0, 2);

                                // Tangent:
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Tangent.X), 0, 2);
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Tangent.Y), 0, 2);
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Tangent.Z), 0, 2);
                                vertexDataStream2.Write(new byte[2], 0, 2);

                                // Texcoord1:
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Texcoord1.X), 0, 2);
                                vertexDataStream2.Write(CompressTexcoord(primitives[i].Vertices[x].Texcoord1.Y), 0, 2);
                                break;
                            }
                        default:
                            {
                                throw new Exception($"Unsupported vertex type '{vertexType}'");
                            }
                    }
                }

                // Update total vertex/index counts.
                header.VerticeCount += primitives[i].Vertices.Count;
                header.IndiceCount += primitives[i].Indices.Count;
            }
            writer.AlignToBoundary(16, 0xCD);

            // Write the primary vertex stream.
            header.VertexData1Offset = (int)writer.BaseStream.Position;
            header.VertexData1Size = (int)vertexDataStream1.Length;
            writer.Write(vertexDataStream1.ToArray(), 0, (int)vertexDataStream1.Length);
            writer.AlignToBoundary(16, 0xCD);

            // Write the secondary vertex stream.
            header.VertexData2Offset = (int)writer.BaseStream.Position;
            header.VertexData2Size = (int)vertexDataStream2.Length;
            writer.Write(vertexDataStream2.ToArray(), 0, (int)vertexDataStream2.Length);
            writer.AlignToBoundary(16, 0xCD);

            // Loop and write indice data.
            header.IndiceDataOffset = (int)writer.BaseStream.Position;
            for (int i = 0; i < primitives.Length; i++)
            {
                for (int x = 0; x < primitives[i].Indices.Count; x++)
                    writer.Write(primitives[i].Indices[x]);
            }
            writer.AlignToBoundary(16, 0xCD);

            // Seek to the start of the file and write the file header.
            writer.BaseStream.Position = 0;
            writer.Write(header.Magic);
            writer.Write(header.Version);
            writer.Write(header.SubVersion);
            writer.Write(header.JointCount);
            writer.Write(header.PrimitiveCount);
            writer.Write(header.MaterialCount);
            writer.Write(header.VerticeCount);
            writer.Write(header.IndiceCount + 1);
            writer.Write(header.PolygonCount);
            writer.Write(header.VertexData1Size);
            writer.Write(header.VertexData2Size);
            writer.Write(header.NumberOfTextures);
            writer.Write(0);
            writer.Write(header.JointDataOffset);
            writer.Write(0);
            writer.Write(header.TextureFilesOffset);
            writer.Write(0);
            writer.Write(header.PrimitiveDataOffset);
            writer.Write(0);
            writer.Write(header.VertexData1Offset);
            writer.Write(0);
            writer.Write(header.VertexData2Offset);
            writer.Write(0);
            writer.Write(header.IndiceDataOffset);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(new Vector3());
            writer.Write(header.BoundaryRadius);
            writer.Write(header.BoundingBoxMin);
            writer.Write(header.BoundingBoxMax);
            writer.Write(header.MidDist);
            writer.Write(header.LowDist);
            writer.Write(header.LightGroup);

            // Close the binary writer.
            writer.Close();

            // Add the model to the archive.
            if (archive.AddFile(this.FilePath, modelStream.ToArray(), out _) == false)
                return false;

            return true;
        }

        static byte[] CompressVertexComponent(float input, float min, float max)
        {
            // Compress the input into the range of [min, max].
            float percent = (input - min) / (max - min);

            return BitConverter.GetBytes((short)((percent * 65535) - 32768));
        }

        static byte[] CompressTexcoord(float input)
        {
            input = Math.Max(Math.Min(input, 1), -1);
            input = (input * 32767) + (input >= 0f ? 0.5f : -0.5f);

            return BitConverter.GetBytes((short)(input >= 0f ? Math.Floor(input) : Math.Ceiling(input)));
        }

        static Vector3 VectorMinimums(params Vector3[] args)
        {
            Vector3 minimum = args[0];

            // Find the minimum vector components amongst all vectors provided.
            for (int i = 1; i < args.Length; i++)
            {
                minimum.X = Math.Min(minimum.X, args[i].X);
                minimum.Y = Math.Min(minimum.Y, args[i].Y);
                minimum.Z = Math.Min(minimum.Z, args[i].Z);
            }

            return minimum;
        }

        static Vector3 VectorMaximums(params Vector3[] args)
        {
            Vector3 maximum = args[0];

            // Find the maximum vector components amongst all vectors provided.
            for (int i = 1; i < args.Length; i++)
            {
                maximum.X = Math.Max(maximum.X, args[i].X);
                maximum.Y = Math.Max(maximum.Y, args[i].Y);
                maximum.Z = Math.Max(maximum.Z, args[i].Z);
            }

            return maximum;
        }
    }
}
