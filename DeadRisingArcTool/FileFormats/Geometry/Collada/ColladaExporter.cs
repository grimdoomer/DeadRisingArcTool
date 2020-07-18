using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
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
                writer.WriteStartElement("COLLADA", "http://www.collada.org/2004/COLLADASchema");
                writer.WriteAttributeString("version", "1.4.1");

                // Write the asset element.
                writer.WriteStartElement("asset");
                    writer.WriteStartElement("contributor");
                        writer.WriteElementString("authoring_tool", "Dead Rising Arc Tool");
                    writer.WriteFullEndElement();
                    writer.WriteElementString("created", DateTime.Now.ToString());
                    writer.WriteElementString("modified", DateTime.Now.ToString());
                writer.WriteFullEndElement();

                // Write the images library element.
                writer.WriteStartElement("library_images");
                {
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
                        writer.WriteAttributeString("name", textureName);
                            writer.WriteElementString("init_from", "./" + textureName + ".dds");
                        writer.WriteFullEndElement();
                    }
                }
                writer.WriteFullEndElement();

                // Write the materials library element.
                //writer.WriteStartElement("library_materials");
                //{
                //    // Loop through all of the materials and write each one.
                //    for (int i = 0; i < model.materials.Length; i++)
                //    {
                //        // Write the material element.
                //        writer.WriteStartElement("material");
                //        writer.WriteAttributeString("id", "Material " + i.ToString());
                //    }
                //}
                //writer.WriteFullEndElement();

                // TODO: Joints

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
            writer.WriteAttributeString("id", "primitive_" + index.ToString());
            writer.WriteAttributeString("name", "Primitive " + index.ToString());
            {
                // Write the mesh element.
                writer.WriteStartElement("mesh");
                {

                }
                writer.WriteFullEndElement();
            }
            writer.WriteFullEndElement();
        }
    }
}
