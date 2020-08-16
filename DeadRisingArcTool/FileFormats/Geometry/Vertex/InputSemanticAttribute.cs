using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.Vertex
{
    public class InputSemanticAttribute : Attribute
    {
        /// <summary>
        /// HLSL semantic associated with this element
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Semantic index for the element
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// Data type of the element
        /// </summary>
        public Format Format { get; private set; }
        /// <summary>
        /// Offset of the element from the start of the vertex
        /// </summary>
        public int Offset { get; private set; }
        /// <summary>
        /// Input assembler slot
        /// </summary>
        public int Slot { get; private set; }

        public InputSemanticAttribute(string name, int index, Format format, int offset, int slot)
        {
            // Initialize fields.
            this.Name = name;
            this.Index = index;
            this.Format = format;
            this.Offset = offset;
            this.Slot = slot;
        }
    }
}
