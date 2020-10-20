using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Misc
{
    public class SerializableXmlStructAttribute : Attribute
    {
        /// <summary>
        /// Resource type this struct represents.
        /// </summary>
        public uint Type { get; private set; }

        /// <summary>
        /// Makes a structure or class serializable to/from binary xml files.
        /// </summary>
        /// <param name="type">Resource type this structure represents</param>
        public SerializableXmlStructAttribute(uint type)
        {
            // Initialize fields.
            this.Type = type;
        }
    }
}
