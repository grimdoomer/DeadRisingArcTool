using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IO
{
    /// <summary>
    /// Allows for streaming of objects to and from an endian stream.
    /// </summary>
    public interface IStreamable
    {
        /// <summary>
        /// Reads the object from the endian stream.
        /// </summary>
        /// <param name="reader">Stream to read from.</param>
        void Read(EndianReader reader);

        /// <summary>
        /// Writes the object to the endian stream.
        /// </summary>
        /// <param name="writer">Stream to write to.</param>
        void Write(EndianWriter writer);
    }
}
