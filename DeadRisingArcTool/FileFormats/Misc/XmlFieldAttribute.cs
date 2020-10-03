using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Misc
{
    public class XmlFieldAttribute : Attribute
    {
        /// <summary>
        /// Name of the xml field
        /// </summary>
        public string FieldName { get; private set; }

        public XmlFieldAttribute(string fieldName)
        {
            // Initialize fields.
            this.FieldName = fieldName;
        }
    }
}
