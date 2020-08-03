using DeadRisingArcTool.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.Controls
{
    /// <summary>
    /// Specifies what resource types an editing control is capable of editing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GameResourceEditorAttribute : Attribute
    {
        /// <summary>
        /// List of resource types this control can edit.
        /// </summary>
        public ResourceType[] ResourceTypes { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType">Type of game resources this control is capable of editing</param>
        public GameResourceEditorAttribute(ResourceType resourceType)
        {
            // Initialize fields.
            this.ResourceTypes = new ResourceType[] { resourceType };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceTypes">Types of game resources this control is capable of editing</param>
        public GameResourceEditorAttribute(params ResourceType[] resourceTypes)
        {
            // Initialize fields.
            this.ResourceTypes = resourceTypes;
        }
    }
}
