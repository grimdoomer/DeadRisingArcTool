using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats.Archive;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rArchive)]
    public partial class ArchiveIndexEditor : GameResourceEditorControl
    {
        public ArchiveIndexEditor()
        {
            InitializeComponent();
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
            {
                // Clear the textbox contents and return.
                this.textBox1.Text = "";
                return;
            }

            // Cast the resource to an rArchive object.
            rArchive archiveIndex = (rArchive)this.GameResource;

            // Create a dictionary of all the files in the archive and their hashcode.
            Dictionary<ulong, string> archiveFiles = new Dictionary<ulong, string>();
            for (int i = 0; i < this.ArcFile.FileEntries.Length; i++)
            {
                // Add the archive file to the dictionary.
                ulong hashcode = rArchive.ComputeHashCode(this.ArcFile.FileEntries[i].FileName.Substring(0, this.ArcFile.FileEntries[i].FileName.IndexOf('.')), 
                    (uint)DeadRisingArcTool.FileFormats.GameResource.KnownResourceTypesReverse[this.ArcFile.FileEntries[i].FileType]);
                archiveFiles.Add(hashcode, this.ArcFile.FileEntries[i].FileName);
            }

            // Loop through all the files in the archive index and print details on what it points to.
            this.textBox1.Text = "";
            for (int i = 0; i < archiveIndex.FileIds.Length; i++)
            {
                // Print the file name of the file this id points to.
                if (archiveFiles.ContainsKey(archiveIndex.FileIds[i]) == true)
                {
                    this.textBox1.Text += string.Format("\t0x{0} -> {1}\r\n", archiveIndex.FileIds[i].ToString("X16"), archiveFiles[archiveIndex.FileIds[i]]);
                }
                else
                {
                    this.textBox1.Text += string.Format("\t0x{0} -> ???\r\n", archiveIndex.FileIds[i].ToString("X16"));
                }
            }
        }

        public override bool SaveResource()
        {
            throw new NotImplementedException();
        }
    }
}
