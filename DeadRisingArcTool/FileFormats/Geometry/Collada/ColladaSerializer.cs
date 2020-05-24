using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DeadRisingArcTool.FileFormats.Geometry.Collada
{
    public class ColladaSerializer
    {
        public static void SerializeDocument(ColladaDocument document, string fileName)
        {
            // Setup xml writer formatting settings.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.NewLineChars = "\r\n";
            settings.Encoding = Encoding.UTF8;

            // Create a new xml writer for the output collada file.
            XmlWriter writer = XmlWriter.Create(fileName, settings);

            // Write the xml header.
            writer.WriteStartDocument();

            // Re-initialize the collada version info.
            document.Version = new ColladaVersion();

            // Get a list of all fields in the collada document.
            FieldInfo[] fields = document.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Loop and serialize all fields to file.
            for (int i = 0; i < fields.Length; i++)
            {
                // Check if the field is null and if so skip it.
                if (fields[i].GetValue(document) == null)
                    continue;

                //
            }

            // Write the end of the xml document and close the file.
            writer.WriteEndDocument();
            writer.Close();
        }

        private static void SerializeField(FieldInfo field, XmlWriter writer)
        {

        }
    }
}
