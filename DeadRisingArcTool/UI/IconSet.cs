using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.UI
{
    public enum UIIcon : int
    {
        File,
        Folder,
        FolderBlue,
        Archive,
        PatchArchive,
        FileCollection,
        FileCollectionBlue,
        Model,
        Texture
    }

    public class IconSet
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IconSet Instance { get; set; } = new IconSet();

        /// <summary>
        /// Image list containing UI icons.
        /// </summary>
        public ImageList IconImageList { get; private set; } = new ImageList();

        public IconSet()
        {
            // Build the image list (must be added in the same order as TreeNodeIcon).
            this.IconImageList.Images.Add(Properties.Resources.File);
            this.IconImageList.Images.Add(Properties.Resources.Folder);
            this.IconImageList.Images.Add(Properties.Resources.FolderBlue);
            this.IconImageList.Images.Add(Properties.Resources.GameArchive);
            this.IconImageList.Images.Add(Properties.Resources.PatchArchive);
            this.IconImageList.Images.Add(Properties.Resources.FileBox);
            this.IconImageList.Images.Add(Properties.Resources.FileBoxBlue);
            this.IconImageList.Images.Add(Properties.Resources.ObjectIcon);
            this.IconImageList.Images.Add(Properties.Resources.Texture);

            // Set additional image list properties.
            this.IconImageList.ColorDepth = ColorDepth.Depth32Bit;
            this.IconImageList.ImageSize = new System.Drawing.Size(18, 18);
        }
    }
}
