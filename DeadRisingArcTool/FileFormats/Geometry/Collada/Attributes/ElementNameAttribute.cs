using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.Collada.Attributes
{
    public class ElementNameAttribute : Attribute
    {
        /// <summary>
        /// Name of the xml element
        /// </summary>
        public string ElementName { get; private set; }

        /// <summary>
        /// URL to the schema description for this element
        /// </summary>
        public string SchemaURL { get; private set; }

        /// <summary>
        /// Initializes a new ElementNameAttribute using the element name provided.
        /// </summary>
        /// <param name="elementName">Name of the xml element</param>
        /// <param name="schemaURL">URL to the schema description for this element</param>
        public ElementNameAttribute(string elementName, string schemaURL = null)
        {
            // Initialize fields.
            this.ElementName = elementName;
            this.SchemaURL = schemaURL;
        }
    }
}
