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
using DeadRisingArcTool.UI.Forms;
using DeadRisingArcTool.FileFormats;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rArchive)]
    public partial class ArchiveIndexEditor : GameResourceEditorControl
    {
        // Dictionary of file ids to file names.
        Dictionary<ulong, string> archiveFiles = new Dictionary<ulong, string>();

        // Reverse dictionary of file names to file ids.
        Dictionary<string, ulong> reverseArchiveFiles = new Dictionary<string, ulong>();

        public ArchiveIndexEditor()
        {
            InitializeComponent();

#if !DEBUG
            // Only enable the Rebuild button if this is a debug build.
            this.btnRebuild.Enabled = false;
#endif
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

            // Clear the lookup dictionaries.
            this.archiveFiles.Clear();
            this.reverseArchiveFiles.Clear();

            // Build the archive file lookup dictionaries.
            for (int i = 0; i < this.ArcFile.FileEntries.Length; i++)
            {
                // Add the archive file to the dictionary.
                ulong hashcode = rArchive.ComputeHashCode(this.ArcFile.FileEntries[i].FileName.Substring(0, this.ArcFile.FileEntries[i].FileName.IndexOf('.')), 
                    (uint)DeadRisingArcTool.FileFormats.GameResource.KnownResourceTypesReverse[this.ArcFile.FileEntries[i].FileType]);
                this.archiveFiles.Add(hashcode, this.ArcFile.FileEntries[i].FileName);
                this.reverseArchiveFiles.Add(this.ArcFile.FileEntries[i].FileName, hashcode);
            }

            // Loop through all the files in the archive index and print details on what it points to.
            this.textBox1.Text = "";
            for (int i = 0; i < archiveIndex.FileIds.Length; i++)
            {
                // Print the file name of the file this id points to.
                if (this.archiveFiles.ContainsKey(archiveIndex.FileIds[i]) == true)
                {
                    this.textBox1.Text += string.Format("\t0x{0} -> {1}\r\n", archiveIndex.FileIds[i].ToString("X16"), this.archiveFiles[archiveIndex.FileIds[i]]);
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

        private void btnRebuild_Click(object sender, EventArgs e)
        {
            // Set the UI state to disabled while we write to file.
            this.EditorOwner.SetUIState(false);

            // Get a list of all the file names from the archive.
            string[] fileNames = this.ArcFile.FileEntries.Select(f => f.FileName).ToArray();

            // Cast the resource to an rArchive object.
            rArchive archiveIndex = (rArchive)this.GameResource;

            // Build a list of tuples for file names and checked status.
            List<Tuple<string, bool>> filesInfo = new List<Tuple<string, bool>>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                // Add an entry fo this file.
                ulong hashcode = this.reverseArchiveFiles[fileNames[i]];
                filesInfo.Add(new Tuple<string, bool>(fileNames[i], archiveIndex.FileIds.Contains(hashcode)));
            }

            // Show the file select dialog.
            FileSelectDialog selectDialog = new FileSelectDialog(filesInfo.ToArray());
            if (selectDialog.ShowDialog() == DialogResult.OK)
            {
                // Loop through all the selected files and create file ids for each one.
                ulong[] fileIds = new ulong[selectDialog.SelectedFiles.Length];
                for (int i = 0; i < selectDialog.SelectedFiles.Length; i++)
                {
                    // Get the resource type from the file extension.
                    string fileExtension = selectDialog.SelectedFiles[i].Substring(selectDialog.SelectedFiles[i].IndexOf('.') + 1);
                    ResourceType resType = (ResourceType)Enum.Parse(typeof(ResourceType), fileExtension);

                    // Calculate the resource id for this file.
                    ulong hashcode = rArchive.ComputeHashCode(selectDialog.SelectedFiles[i].Substring(0, selectDialog.SelectedFiles[i].IndexOf('.')),
                        (uint)DeadRisingArcTool.FileFormats.GameResource.KnownResourceTypesReverse[resType]);
                    fileIds[i] = hashcode;
                }

                // Set the list of file entries in the archive index instnace.
                archiveIndex.FileIds = fileIds.ToArray();

                // Save the new archive index to back to the archive.
                if (ArchiveCollection.Instance.InjectFile(new DatumIndex[] { archiveIndex.Datum }, archiveIndex.ToBuffer()) == false)
                {
                    // Failed to save archive index.
                    this.EditorOwner.SetUIState(true);
                    MessageBox.Show("Failed to rebuild archive index!");
                    return;
                }

                // Update the UI to reflect changes.
                OnGameResourceUpdated();

                // Archive index rebuilt successfully.
                MessageBox.Show("Archive index successfully rebuilt!");
            }

            // Re-enable the UI.
            this.EditorOwner.SetUIState(true);
        }
    }
}
