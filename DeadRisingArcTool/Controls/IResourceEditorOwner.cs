using DeadRisingArcTool.FileFormats.Archive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.Controls
{
    public interface IResourceEditorOwner
    {
        /// <summary>
        /// Sets the UI state to be enabled or disabled
        /// </summary>
        /// <param name="enabled">True if the UI should be enabled, false if it should be disabled</param>
        void SetUIState(bool enabled);

        /// <summary>
        /// Gets a list of duplicate datums for duplicate arc files that should be updated with the modified resource data
        /// </summary>
        /// <param name="fileName">Name of the arc file to search for duplicates of</param>
        /// <returns>Array of datums for the duplicate files</returns>
        DatumIndex[] GetDatumsToUpdateForResource(string fileName);
    }
}
