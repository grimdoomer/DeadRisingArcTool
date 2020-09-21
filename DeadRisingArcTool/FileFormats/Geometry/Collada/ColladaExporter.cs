using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.FileFormats.Geometry.Vertex;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DeadRisingArcTool.FileFormats.Geometry.Collada
{
    public class ColladaExporter
    {
        public static bool ExportModel(rModel model, string outputFolder)
        {
            // Setup xml writer formatting settings.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.Encoding = Encoding.UTF8;

            // Create a new xml writer for the output collada file.
            string modelName = Path.GetFileName(model.FileName).Replace(".rModel", "");
            using (XmlWriter writer = XmlWriter.Create(string.Format("{0}\\{1}.dae", outputFolder, modelName), settings))
            {
                // Write the xml header.
                writer.WriteStartDocument();

                // Write the collada element start.
                writer.WriteStartElement("COLLADA", "http://www.collada.org/2005/11/COLLADASchema");
                writer.WriteAttributeString("version", "1.4.1");

                // Write the asset element.
                writer.WriteStartElement("asset");

                    // Contributer block:
                    writer.WriteStartElement("contributor");
                        writer.WriteElementString("authoring_tool", "Dead Rising Arc Tool");
                    writer.WriteFullEndElement();

                    // Modification timestamps:
                    writer.WriteElementString("created", DateTime.Now.ToString());
                    writer.WriteElementString("modified", DateTime.Now.ToString());

                    // World units block:
                    writer.WriteStartElement("unit");
                        writer.WriteAttributeString("name", "meter");
                        writer.WriteAttributeString("meter", "1");
                    writer.WriteEndElement();

                    // Up axis block:
                    writer.WriteElementString("up_axis", "Y_UP");
                writer.WriteFullEndElement();

                #region library_images

                // Write the images library element.
                writer.WriteStartElement("library_images");
                {
                    // Write the first image as empty.
                    writer.WriteStartElement("image");
                    writer.WriteAttributeString("id", "null");
                    writer.WriteAttributeString("name", "null");
                    {
                        writer.WriteElementString("init_from", "null.dds");
                    }
                    writer.WriteFullEndElement();

                    // Loop and write and image element for every texture the model has.
                    for (int i = 0; i < model.textureFileNames.Length; i++)
                    {
                        // Get name of the texture.
                        string textureName = Path.GetFileName(model.textureFileNames[i]);

                        // Find the arc file the resource is in.
                        string textureFileName = GameResource.GetFullResourceName(model.textureFileNames[i], ResourceType.rTexture);
                        ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(textureFileName, out Archive.Archive arcFile, out ArchiveFileEntry fileEntry);
                        if (arcFile == null || fileEntry == null)
                        {
                            // TODO: The file can also be a r2Texture.
                            continue;

                            // Failed to find a resource with the specified name.
                            return false;
                        }

                        // Parse the game resource and cast it to rtexture.
                        rTexture texture = arcFile.GetFileAsResource<rTexture>(textureFileName);

                        // Save the texture to a dds image that can be loaded with the model.
                        DDSImage ddsImage = DDSImage.FromGameTexture(texture);
                        if (ddsImage.WriteToFile(string.Format("{0}\\{1}.dds", outputFolder, textureName)) == false)
                        {
                            // Failed to extract texture to file.
                            return false;
                        }

                        // Write the image element.
                        writer.WriteStartElement("image");
                        writer.WriteAttributeString("id", textureName);
                        writer.WriteAttributeString("name", textureName);
                        {
                            writer.WriteElementString("init_from", textureName + ".dds");
                        }
                        writer.WriteFullEndElement();
                    }
                }
                writer.WriteFullEndElement();

                #endregion

                #region library_effects

                // Write the effects library element.
                writer.WriteStartElement("library_effects");
                {
                    // Loop through all of the materials and create an effect for each one.
                    for (int i = 0; i < model.materials.Length; i++)
                    {
                        // Write the effect element.
                        writer.WriteStartElement("effect");
                        writer.WriteAttributeString("id", string.Format("material-{0}-effect", i.ToString()));
                        {
                            // Write the common profile element.
                            writer.WriteStartElement("profile_COMMON");
                            {
                                // Format some object ids.
                                bool hasNormalMap = model.materials[i].BaseMapTexture != 0;
                                string baseMapTexture = model.materials[i].BaseMapTexture == 0 ? "null" : Path.GetFileName(model.textureFileNames[model.materials[i].BaseMapTexture - 1]);
                                string normalMapTexture = model.materials[i].NormalMapTexture == 0 ? "null" : Path.GetFileName(model.textureFileNames[model.materials[i].NormalMapTexture - 1]);
                                string surfaceId = string.Format("material-{0}-surface", i.ToString());
                                string samplerId = string.Format("material-{0}-sampler", i.ToString());
                                string texcoordsId = string.Format("material-{0}-texcoords", i.ToString());

                                // Write a new param block for the surface.
                                writer.WriteStartElement("newparam");
                                writer.WriteAttributeString("sid", surfaceId + "_basemap");
                                    writer.WriteStartElement("surface");
                                    writer.WriteAttributeString("type", "2D");
                                        writer.WriteElementString("init_from", baseMapTexture);
                                    writer.WriteEndElement();
                                writer.WriteEndElement();

                                // Write the new param block for the sampler.
                                writer.WriteStartElement("newparam");
                                writer.WriteAttributeString("sid", samplerId + "_basemap");
                                    writer.WriteStartElement("sampler2D");
                                        writer.WriteElementString("source", surfaceId + "_basemap");
                                    writer.WriteEndElement();
                                writer.WriteEndElement();

                                // Check if the material has a normal map and if so write the surface and sampler for it.
                                if (hasNormalMap == true)
                                {
                                    // Write the normal map surface.
                                    writer.WriteStartElement("newparam");
                                    writer.WriteAttributeString("sid", surfaceId + "_bumpmap");
                                        writer.WriteStartElement("surface");
                                        writer.WriteAttributeString("type", "2D");
                                            writer.WriteElementString("init_from", normalMapTexture);
                                        writer.WriteEndElement();
                                    writer.WriteEndElement();

                                    // Write the normal map sampler.
                                    writer.WriteStartElement("newparam");
                                    writer.WriteAttributeString("sid", samplerId + "_bumpmap");
                                        writer.WriteStartElement("sampler2D");
                                            writer.WriteElementString("source", surfaceId + "_bumpmap");
                                        writer.WriteEndElement();
                                    writer.WriteEndElement();
                                }

                                // Write the technique element.
                                writer.WriteStartElement("technique");
                                writer.WriteAttributeString("sid", "common");
                                {
                                    // Write the lambert element.
                                    writer.WriteStartElement("lambert");
                                    {
                                        // Write the emission color.
                                        writer.WriteStartElement("emission");
                                            writer.WriteStartElement("color");
                                            writer.WriteAttributeString("sid", "emission");
                                                writer.WriteString("0 0 0 1");
                                            writer.WriteEndElement();
                                        writer.WriteEndElement();

                                        // Write the diffuse texture.
                                        writer.WriteStartElement("diffuse");
                                            writer.WriteStartElement("texture");
                                            writer.WriteAttributeString("texture", samplerId + "_basemap");
                                            writer.WriteAttributeString("texcoord", texcoordsId + "0");
                                            writer.WriteEndElement();
                                        writer.WriteEndElement();

                                        // Write index of refraction block for blender.
                                        writer.WriteStartElement("index_of_refraction");
                                            writer.WriteStartElement("float");
                                            writer.WriteAttributeString("sid", "ior");
                                                writer.WriteString("1.45");
                                            writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    }
                                    writer.WriteEndElement();

                                    // Check if the model has a bump map and if so write the bump technique.
                                    if (hasNormalMap == true)
                                    {
                                        // Write the bump map technique.
                                        writer.WriteStartElement("extra");
                                            writer.WriteStartElement("technique");
                                            writer.WriteAttributeString("profile", "FCOLLADA");
                                                writer.WriteStartElement("bump");
                                                    writer.WriteStartElement("texture");
                                                    writer.WriteAttributeString("texture", samplerId + "_bumpmap");
                                                    writer.WriteAttributeString("texcoord", texcoordsId + "1");
                                                    writer.WriteEndElement();
                                                writer.WriteEndElement();
                                            writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                #endregion

                #region library_materials

                // Write the materials library element.
                writer.WriteStartElement("library_materials");
                {
                    // Loop through all of the materials and write each one.
                    for (int i = 0; i < model.materials.Length; i++)
                    {
                        // Format object ids.
                        string materialId = "material-" + i.ToString();
                        string effectId = string.Format("#material-{0}-effect", i.ToString());

                        // Write the material element.
                        writer.WriteStartElement("material");
                        writer.WriteAttributeString("id", materialId);
                        writer.WriteAttributeString("name", "Material " + i.ToString());
                            writer.WriteStartElement("instance_effect");
                            writer.WriteAttributeString("url", effectId);
                            writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                #endregion

                // TODO: Joints

                #region library_geometries

                // Write the geometries library element.
                writer.WriteStartElement("library_geometries");
                {
                    // Loop through all the primitives and write each one.
                    for (int i = 0; i < model.primitives.Length; i++)
                    {
                        // Write the primitive to file.
                        WritePrimitiveBlock(model, writer, i);
                    }
                }
                writer.WriteFullEndElement();

                #endregion

                #region library_visual_scenes

                // Write the visual scenes library element.
                writer.WriteStartElement("library_visual_scenes");
                {
                    // Write the visual scene element.
                    writer.WriteStartElement("visual_scene");
                    writer.WriteAttributeString("id", modelName + "-scene");
                    writer.WriteAttributeString("name", modelName + "-scene");
                    {
                        // Write the node element for the model.
                        writer.WriteStartElement("node");
                        writer.WriteAttributeString("id", modelName);
                        writer.WriteAttributeString("name", modelName);
                        writer.WriteAttributeString("type", "NODE");
                        {
                            // Write the transform matrix element.
                            WriteMatrixElement(writer, "transform", Matrix.Identity);

                            // Loop through all the primitives and create instance geometry nodes for each one.
                            for (int i = 0; i < model.primitives.Length; i++)
                            {
                                // Write the instance geometry element.
                                writer.WriteStartElement("instance_geometry");
                                string primitiveId = "primitive-" + i.ToString();
                                writer.WriteAttributeString("url", "#" + primitiveId);
                                writer.WriteAttributeString("name", primitiveId);
                                {
                                    // Write the bind material block.
                                    writer.WriteStartElement("bind_material");
                                        writer.WriteStartElement("technique_common");
                                            writer.WriteStartElement("instance_material");
                                            writer.WriteAttributeString("symbol", "material-" + model.primitives[i].MaterialIndex.ToString());
                                            writer.WriteAttributeString("target", "#material-" + model.primitives[i].MaterialIndex.ToString());

                                                // Basemap:
                                                writer.WriteStartElement("bind_vertex_input");
                                                writer.WriteAttributeString("semantic", string.Format("material-{0}-texcoords0", model.primitives[i].MaterialIndex.ToString()));
                                                writer.WriteAttributeString("input_semantic", "TEXCOORD");
                                                writer.WriteAttributeString("input_set", "0");
                                                writer.WriteEndElement();

                                                // Check if the material has a bump map.
                                                if (model.materials[model.primitives[i].MaterialIndex].NormalMapTexture != 0)
                                                {
                                                    // Normal map:
                                                    writer.WriteStartElement("bind_vertex_input");
                                                    writer.WriteAttributeString("semantic", string.Format("material-{0}-texcoords1", model.primitives[i].MaterialIndex.ToString()));
                                                    writer.WriteAttributeString("input_semantic", "TEXCOORD");
                                                    writer.WriteAttributeString("input_set", "1");
                                                    writer.WriteEndElement();
                                                }
                                            writer.WriteEndElement();
                                        writer.WriteEndElement();
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                #endregion

                // Write the scene element.
                writer.WriteStartElement("scene");
                    writer.WriteStartElement("instance_visual_scene");
                    writer.WriteAttributeString("url", "#" + modelName + "-scene");
                    writer.WriteEndElement();
                writer.WriteEndElement();

                // Write the collada and document end.
                writer.WriteEndElement();
                writer.WriteEndDocument();

                // Flush to file.
                writer.Close();
                return true;
            }
        }

        private static void WritePrimitiveBlock(rModel model, XmlWriter writer, int index)
        {
            // Write the geometry element.
            writer.WriteStartElement("geometry");
            string primitiveId = "primitive-" + index.ToString();
            writer.WriteAttributeString("id", primitiveId);
            writer.WriteAttributeString("name", "Primitive " + index.ToString());
            {
                // Write the mesh element.
                writer.WriteStartElement("mesh");
                {
                    // Check the material flags for this primitive to determine the shader type.
                    int shaderType = (model.materials[model.primitives[index].MaterialIndex].Flags >> 27) & 7;
                    switch (shaderType)
                    {
                        case 0: WriteVertexStream(writer, model, SkinnedRigid4WMesh.VertexFormat, index, primitiveId, true); break;
                        case 1: WriteVertexStream(writer, model, SkinnedRigid8WMesh.VertexFormat, index, primitiveId, true); break;
                        case 2: WriteVertexStream(writer, model, LevelGeometry1Shader.VertexFormat, index, primitiveId, true); break;
                        default:
                            {
                                // Shader type not currently supported.
                                throw new NotSupportedException(string.Format("Shader type {0} not currently supported!", shaderType));
                            }
                    }
                }
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();
        }

        private static void WriteVertexStream(XmlWriter writer, rModel model, InputElement[] vertexFormat, int primitiveIndex, string primitiveId, bool positionCompressed)
        {
            // Compute the quantize position and scale for decompression.
            Vector4 gXfQuantPosScale = model.header.BoundingBoxMax - model.header.BoundingBoxMin;
            Vector4 gXfQuantPosOffset = model.header.BoundingBoxMin;

            // Format the ids for the vertex data elements.
            string positionsId = "";
            string verticesId = string.Format("{0}-vertices", primitiveId);
            string materialId = string.Format("material-{0}", model.primitives[primitiveIndex].MaterialIndex);

            // Loop through all of the components in the vertex declaration and process each one.
            for (int i = 0; i < vertexFormat.Length; i++)
            {
                // Setup vertex stream info.
                int vertexStride = model.primitives[primitiveIndex].VertexStride1;
                int vertexStartOffset = model.primitives[primitiveIndex].VertexStream1Offset;
                byte[] vertexData = model.vertexData1;

                // Check the slot number for the vertex component.
                if (vertexFormat[i].Slot == 1)
                {
                    // Use the secondary vertex stream.
                    vertexStride = model.primitives[primitiveIndex].VertexStride2;
                    vertexStartOffset = model.primitives[primitiveIndex].VertexStream2Offset;
                    vertexData = model.vertexData2;
                }

                // Calculate the starting offset based on the selected vertex stride.
                int startOffset = vertexStartOffset + (model.primitives[primitiveIndex].StartingVertex + model.primitives[primitiveIndex].BaseVertexLocation) * vertexStride;

                // If this is a texcoord semantic make sure the bitmap is actually used.
                if (vertexFormat[i].SemanticName == "TEXCOORD")
                {
                    // Check to make sure the bitmap is actually used.
                    if (vertexFormat[i].Slot == 0 && vertexFormat[i].AlignedByteOffset == 0)
                        continue;
                }
                else if (vertexFormat[i].SemanticName == "BLENDWEIGHT" || vertexFormat[i].SemanticName == "BLENDINDICES")
                {
                    // Blend info goes into a different section.
                    continue;
                }

                // Format ids for the current component.
                string componentId = string.Format("{0}-{1}{2}", primitiveId, vertexFormat[i].SemanticName.ToLower(), vertexFormat[i].SemanticIndex);
                string componentArrayId = componentId + "-array";

                // If this is the POSITION element save the id for later.
                if (vertexFormat[i].SemanticName == "POSITION")
                    positionsId = componentId;

                // Write the source block for this vertex component.
                writer.WriteStartElement("source");
                writer.WriteAttributeString("id", componentId);
                {
                    Type vectorType = null;

                    // Write the float array element start.
                    writer.WriteStartElement("float_array");
                    writer.WriteAttributeString("id", componentArrayId);
                    writer.WriteAttributeString("count", (model.primitives[primitiveIndex].VertexCount * 2).ToString());
                    {
                        // Loop and unpack the vector data.
                        for (int x = 0; x < model.primitives[primitiveIndex].VertexCount; x++)
                        {
                            // Calculate the starting offset in the vertex data buffer.
                            int offset = startOffset + (x * vertexStride) + vertexFormat[i].AlignedByteOffset;

                            // Check the component type and handle accordingly.
                            switch (vertexFormat[i].SemanticName)
                            {
                                #region POSITION

                                case "POSITION":
                                    {
                                        // Unpack the vertex from the stream.
                                        Vector4 position = new Vector4(0.0f);
                                        switch (vertexFormat[i].Format)
                                        {
                                            case SharpDX.DXGI.Format.R32G32B32_Float: position = new Vector4(VertexHelper.Unpack_R32G32B32_Float(vertexData, offset), 0.0f); break;
                                            case SharpDX.DXGI.Format.R16G16B16A16_SNorm: position = VertexHelper.Unpack_R16G16B16A16_SNorm(vertexData, offset); break;
                                            default:
                                                {
                                                    // Vertex format is not supported.
                                                    throw new NotSupportedException(string.Format("Vertex format {0} is not currently supported!", vertexFormat[i].Format));
                                                }
                                        }

                                        // Decompress using bounding box quantization.
                                        if (positionCompressed == true)
                                            position = position * gXfQuantPosScale + gXfQuantPosOffset;

                                        // Write the position vector.
                                        writer.WriteString(string.Format("{0} {1} {2} ", position.X, position.Y, position.Z));

                                        // Set the vector type.
                                        vectorType = typeof(Vector3);
                                        break;
                                    }

                                #endregion

                                #region TEXCOORD

                                case "TEXCOORD":
                                    {
                                        // Unpack the texcoord from the stream.
                                        Vector2 texcoord = new Vector2(0.0f);
                                        switch (vertexFormat[i].Format)
                                        {
                                            case SharpDX.DXGI.Format.R16G16_SNorm: texcoord = VertexHelper.Unpack_R16G16_SNorm(vertexData, offset); break;
                                            case SharpDX.DXGI.Format.R32G32_Float: texcoord = VertexHelper.Unpack_R32G32_Float(vertexData, offset); break;
                                            default:
                                                {
                                                    // Vertex format not currently supported.
                                                    throw new NotSupportedException(string.Format("Vertex format {0} is not currently supported!", vertexFormat[i].Format));
                                                }
                                        }

                                        // Flip the the UVs over the y-axis to correct for lower-left origin.
                                        texcoord.Y = 1.0f - texcoord.Y;

                                        // Write the texcoord to file.
                                        writer.WriteString(string.Format("{0} {1} ", texcoord.X, texcoord.Y));

                                        // Set the vector type.
                                        vectorType = typeof(Vector2);
                                        break;
                                    }

                                #endregion

                                #region NORMAL/TANGENT

                                case "NORMAL":
                                case "TANGENT":
                                    {
                                        // Unpack the normal vector from the stream.
                                        Vector4 normal = new Vector4(0.0f);
                                        switch (vertexFormat[i].Format)
                                        {
                                            case SharpDX.DXGI.Format.R16G16B16A16_SNorm: normal = VertexHelper.Unpack_R16G16B16A16_SNorm(vertexData, offset); break;
                                            default:
                                                {
                                                    // Vertex format is not supported.
                                                    throw new NotSupportedException(string.Format("Vertex format {0} is not currently supported!", vertexFormat[i].Format));
                                                }
                                        }

                                        // Write the normal vector.
                                        writer.WriteString(string.Format("{0} {1} {2}", normal.X, normal.Y, normal.Z));

                                        // Set the vector type.
                                        vectorType = typeof(Vector3);
                                        break;
                                    }

                                    #endregion
                            }
                        }
                    }
                    writer.WriteFullEndElement();

                    // Write the technique element.
                    writer.WriteStartElement("technique_common");
                    {
                        // Write the accessor element.
                        WriteAccessorElement(writer, componentArrayId, model.primitives[primitiveIndex].VertexCount, vectorType);
                    }
                    writer.WriteFullEndElement();
                }
                writer.WriteFullEndElement();
            }

            // Write the vertices element.
            writer.WriteStartElement("vertices");
            writer.WriteAttributeString("id", verticesId);
            {
                // Write the position input semantic.
                WriteInputSemantic(writer, "POSITION", "#" + positionsId);
            }
            writer.WriteFullEndElement();

            // Convert the triangle strip to a triangle list for better compatibility (blender).
            ushort[] triangleIndices = VertexHelper.TriangleStripToTriangleList(model.indexData, model.primitives[primitiveIndex].StartingIndexLocation, 
                model.primitives[primitiveIndex].IndexCount, model.primitives[primitiveIndex].StartingVertex);

            // Write the triangles element.
            writer.WriteStartElement("triangles");
            writer.WriteAttributeString("material", materialId);
            writer.WriteAttributeString("count", (triangleIndices.Length * 2).ToString());
            {
                // Write the input semantics for the vertices.
                WriteInputSemantic(writer, "VERTEX", "#" + verticesId, 0);

                // Loop through the vertex format and write each semantic that is used.
                int semanticsUsed = 1;
                for (int i = 0; i < vertexFormat.Length; i++)
                {
                    // Skip the POSITION semantic because we already handled it.
                    if (vertexFormat[i].SemanticName == "POSITION" || vertexFormat[i].SemanticName == "BLENDWEIGHT" || vertexFormat[i].SemanticName == "BLENDINDICES")
                        continue;

                    // If this is a texcoord semantic make sure the bitmap is actually used.
                    if (vertexFormat[i].SemanticName == "TEXCOORD")
                    {
                        // Check to make sure the bitmap is actually used.
                        if (vertexFormat[i].Slot == 0 && vertexFormat[i].AlignedByteOffset == 0)
                            continue;
                    }

                    // Write the semantic info.
                    string componentId = string.Format("{0}-{1}{2}", primitiveId, vertexFormat[i].SemanticName.ToLower(), vertexFormat[i].SemanticIndex);
                    WriteInputSemantic(writer, vertexFormat[i].SemanticName, "#" + componentId, semanticsUsed); // This might break texcoords without the set attribute
                    semanticsUsed++;
                }

                // Write triangle indices.
                writer.WriteStartElement("p");
                {
                    // Write the triangle indices.
                    for (int i = 0; i < triangleIndices.Length; i++)
                    {
                        // Write the index for the number of semantics used. Blender does not support loading multiple
                        // semantics with the same indices, so we must duplicate them (lame).
                        for (int x = 0; x < semanticsUsed; x++)
                            writer.WriteString(triangleIndices[i].ToString() + " ");
                    }
                }
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();
        }

        #region Helpers

        private static void WriteAccessorElement(XmlWriter writer, string id, int elementCount, Type elementType)
        {
            // Get the number of components from the vector type.
            int components = 0;
            if (elementType == typeof(Vector2))
                components = 2;
            else if (elementType == typeof(Vector3))
                components = 3;
            else if (elementType == typeof(Vector4))
                components = 4;
            else
                throw new ArgumentException("element type is not supported!");

            // Write the accessor element start.
            writer.WriteStartElement("accessor");
            writer.WriteAttributeString("source", id);
            writer.WriteAttributeString("count", elementCount.ToString());
            writer.WriteAttributeString("stride", components.ToString());
            {
                // Write vector component info.
                if (components == 2)
                {
                    writer.WriteStartElement("param"); writer.WriteAttributeString("name", "U"); writer.WriteAttributeString("type", "float"); writer.WriteEndElement();
                    writer.WriteStartElement("param"); writer.WriteAttributeString("name", "V"); writer.WriteAttributeString("type", "float"); writer.WriteEndElement();
                }
                else if (components >= 3)
                {
                    writer.WriteStartElement("param"); writer.WriteAttributeString("name", "X"); writer.WriteAttributeString("type", "float"); writer.WriteEndElement();
                    writer.WriteStartElement("param"); writer.WriteAttributeString("name", "Y"); writer.WriteAttributeString("type", "float"); writer.WriteEndElement();
                    writer.WriteStartElement("param"); writer.WriteAttributeString("name", "Z"); writer.WriteAttributeString("type", "float"); writer.WriteEndElement();

                    if (components == 4)
                    {
                        writer.WriteStartElement("param"); writer.WriteAttributeString("name", "W"); writer.WriteAttributeString("type", "float"); writer.WriteEndElement();
                    }
                }
            }
            writer.WriteFullEndElement();
        }

        private static void WriteInputSemantic(XmlWriter writer, string semantic, string sourceId, int offset = -1, int set = -1)
        {
            // Write input semantic element.
            writer.WriteStartElement("input");
            writer.WriteAttributeString("semantic", semantic);
            writer.WriteAttributeString("source", sourceId);

            if (offset != -1)
                writer.WriteAttributeString("offset", offset.ToString());

            if (set != -1)
                writer.WriteAttributeString("set", set.ToString());

            writer.WriteEndElement();
        }

        private static void WriteMatrixElement(XmlWriter writer, string sid, Matrix matrix)
        {
            // Write the matrix element.
            writer.WriteStartElement("matrix");
            writer.WriteAttributeString("sid", sid);
            writer.WriteString(string.Format("{0} {1} {2} {3}", matrix.Row1.X, matrix.Row1.Y, matrix.Row1.Z, matrix.Row1.W));
            writer.WriteString(string.Format(" {0} {1} {2} {3}", matrix.Row2.X, matrix.Row2.Y, matrix.Row2.Z, matrix.Row2.W));
            writer.WriteString(string.Format(" {0} {1} {2} {3}", matrix.Row3.X, matrix.Row3.Y, matrix.Row3.Z, matrix.Row3.W));
            writer.WriteString(string.Format(" {0} {1} {2} {3}", matrix.Row4.X, matrix.Row4.Y, matrix.Row4.Z, matrix.Row4.W));
            writer.WriteEndElement();
        }

        #endregion
    }
}
