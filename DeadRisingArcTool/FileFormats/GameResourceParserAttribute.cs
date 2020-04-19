using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats
{
    /// <summary>
    /// Specifies the resource types this parser can read.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GameResourceParserAttribute : Attribute
    {
        /// <summary>
        /// Resource types this parser can read.
        /// </summary>
        public ResourceType[] ResourceTypes { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType">Resource type this parser can read</param>
        public GameResourceParserAttribute(ResourceType resourceType)
        {
            // Initialize fields.
            this.ResourceTypes = new ResourceType[] { resourceType };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceTypes">Resource types this parser can read</param>
        public GameResourceParserAttribute(params ResourceType[] resourceTypes)
        {
            // Initialize fields.
            this.ResourceTypes = resourceTypes;
        }
    }
}
