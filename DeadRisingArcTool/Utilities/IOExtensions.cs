using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.Utilities
{
    public static class IOExtensions
    {
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            string stringValue = "";

            // Loop and read until we hit the null terminator.
            char c = '\0';
            while ((c = reader.ReadChar()) != '\0')
                stringValue += c;

            // Return the string.
            return stringValue;
        }

        public static void AlignToBoundary(this BinaryWriter writer, int alignment)
        {
            AlignToBoundary(writer, alignment, 0x00);
        }

        public static void AlignToBoundary(this BinaryWriter writer, int alignment, byte value)
        {
            // Compute the padding size.
            int padding = alignment - ((int)writer.BaseStream.Position % alignment);

            // Check if it is necessary to write any padding.
            if (padding != alignment)
            {
                // Write padding to stream.
                for (int i = 0; i < padding; i++)
                    writer.Write(value);
            }
        }

        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public static void Write(this BinaryWriter writer, Vector4 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, Matrix value)
        {
            float[] components = value.ToArray();
            for (int i = 0; i < components.Length; i++)
                writer.Write(components[i]);
        }
    }
}
