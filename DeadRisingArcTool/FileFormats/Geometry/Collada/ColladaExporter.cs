using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using SharpDX;
using System;
using System.Collections.Generic;
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
                                string textureName = model.materials[i].BaseMapTexture == 0 ? "null" : Path.GetFileName(model.textureFileNames[model.materials[i].BaseMapTexture - 1]);
                                string surfaceId = string.Format("material-{0}-surface", i.ToString());
                                string samplerId = string.Format("material-{0}-sampler", i.ToString());
                                string texcoordsId = string.Format("material-{0}-texcoords", i.ToString());

                                // Write a new param block for the surface.
                                writer.WriteStartElement("newparam");
                                writer.WriteAttributeString("sid", surfaceId);
                                    writer.WriteStartElement("surface");
                                    writer.WriteAttributeString("type", "2D");
                                        writer.WriteElementString("init_from", textureName);
                                    writer.WriteEndElement();
                                writer.WriteEndElement();

                                // Write the new param block for the sampler.
                                writer.WriteStartElement("newparam");
                                writer.WriteAttributeString("sid", samplerId);
                                    writer.WriteStartElement("sampler2D");
                                        writer.WriteElementString("source", surfaceId);
                                    writer.WriteEndElement();
                                writer.WriteEndElement();

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
                                            writer.WriteAttributeString("texture", samplerId);
                                            writer.WriteAttributeString("texcoord", texcoordsId);
                                            writer.WriteEndElement();
                                        writer.WriteEndElement();

                                        // Write index of refraction block for blender.
                                        writer.WriteStartElement("index_of_refraction");
                                            writer.WriteStartElement("float");
                                            writer.WriteAttributeString("sid", "ior");
                                            writer.WriteString("1.45");
                                        writer.WriteEndElement();
                                    }
                                    writer.WriteEndElement();
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
                                                writer.WriteStartElement("bind_vertex_input");
                                                writer.WriteAttributeString("semantic", string.Format("material-{0}-texcoords", model.primitives[i].MaterialIndex.ToString()));
                                                writer.WriteAttributeString("input_semantic", "TEXCOORD");
                                                writer.WriteAttributeString("input_set", "0");
                                                writer.WriteEndElement();
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
                        case 0: WriteShaderType0VertexStream(model, writer, index, primitiveId); break;
                        default: System.Diagnostics.Debug.Assert(false); break;
                    }
                }
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();
        }

        private static void WriteShaderType0VertexStream(rModel model, XmlWriter writer, int index, string id)
        {
            // Compute the quantize position and scale for decompression.
            Vector4 gXfQuantPosScale = model.header.BoundingBoxMax - model.header.BoundingBoxMin;
            Vector4 gXfQuantPosOffset = model.header.BoundingBoxMin;

            #region POSITION

            // Write the positions block.
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", id + "-positions");
            writer.WriteAttributeString("name", id + "-positions");
            {
                // Write the positions array element start.
                int vertCount = model.primitives[index].VertexCount;
                writer.WriteStartElement("float_array");
                writer.WriteAttributeString("id", id + "-positions-array");
                writer.WriteAttributeString("count", (vertCount * 3).ToString());
                {
                    // Loop and decompress the vertex positions.
                    int startOffset = model.primitives[index].StartingVertex * model.primitives[index].VertexStride1;
                    for (int i = 0; i < vertCount; i++)
                    {
                        // Unpack the vertex from the stream.
                        Vector4 position = VertexHelper.Decompress_R16G16B16A16_SNorm(model.vertexData1, startOffset + (i * model.primitives[index].VertexStride1));

                        // Decompress using bounding box quantization.
                        position = position * gXfQuantPosScale + gXfQuantPosOffset;

                        writer.WriteString(string.Format("{0} {1} {2} ", position.X, position.Y, position.Z));
                    }
                }
                writer.WriteFullEndElement();

                // Write the technique element.
                writer.WriteStartElement("technique_common");
                {
                    // Write the accessor element.
                    WriteAccessorElement(writer, string.Format("#{0}-positions-array", id), vertCount, typeof(Vector3));
                }
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();

            #endregion

            // BLENDINDICES

            // BLENDWEIGHT

            // NORMAL

            #region TEXCOORD

            // Write the positions block.
            writer.WriteStartElement("source");
            writer.WriteAttributeString("id", id + "-texcoords0");
            {
                // Write the positions array element start.
                writer.WriteStartElement("float_array");
                writer.WriteAttributeString("id", id + "-texcoords0-array");
                writer.WriteAttributeString("count", (model.primitives[index].VertexCount * 2).ToString());
                {
                    // Loop and decompress the texcoords.
                    int startOffset = model.primitives[index].StartingVertex * model.primitives[index].VertexStride1;
                    for (int i = 0; i < model.primitives[index].VertexCount; i++)
                    {
                        // Unpack the texcoord from the stream.
                        Vector2 texcoord = VertexHelper.Decompress_R16G16_SNorm(model.vertexData1, startOffset + (i * model.primitives[index].VertexStride1) + 24);

                        // Flip the the UVs over the y-axis to correct for lower-left origin.
                        texcoord.Y = 1.0f - texcoord.Y;

                        writer.WriteString(string.Format("{0} {1} ", texcoord.X, texcoord.Y));
                    }
                }
                writer.WriteFullEndElement();

                // Write the technique element.
                writer.WriteStartElement("technique_common");
                {
                    // Write the accessor element.
                    WriteAccessorElement(writer, string.Format("#{0}-texcoords0-array", id), model.primitives[index].VertexCount, typeof(Vector2));
                }
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();

            #endregion

            // Write the vertices element.
            writer.WriteStartElement("vertices");
            writer.WriteAttributeString("id", id + "-vertices");
            {
                // Write the position input semantic.
                WriteInputSemantic(writer, "POSITION", string.Format("#{0}-positions", id));
            }
            writer.WriteFullEndElement();

            // Convert the triangle strip to a triangle list for better compatibility (blender).
            short[] triangleIndices = VertexHelper.TriangleStripToTriangleList(model.indexData, 
                model.primitives[index].StartingIndexLocation, model.primitives[index].IndexCount, model.primitives[index].StartingVertex);

            // Write the triangles element.
            writer.WriteStartElement("triangles");
            writer.WriteAttributeString("material", string.Format("material-{0}", model.primitives[index].MaterialIndex.ToString()));
            writer.WriteAttributeString("count", (triangleIndices.Length * 2).ToString());
            {
                // Write the input semantics.
                WriteInputSemantic(writer, "VERTEX", string.Format("#{0}-vertices", id), 0);
                WriteInputSemantic(writer, "TEXCOORD", string.Format("#{0}-texcoords0", id), 1, 0);

                // Write triangle indices.
                writer.WriteStartElement("p");
                {
                    // Write the triangle indices.
                    for (int i = 0; i < triangleIndices.Length; i++)
                        writer.WriteString(triangleIndices[i].ToString() + " " + triangleIndices[i].ToString() + " ");
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
