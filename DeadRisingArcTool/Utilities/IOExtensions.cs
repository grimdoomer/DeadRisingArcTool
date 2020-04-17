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
    }
}
