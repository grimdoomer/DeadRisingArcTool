using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats;
using DeadRisingArcTool.FileFormats.Misc;
using DeadRisingArcTool.FileFormats.Archive;
using System.Reflection;
using SharpDX;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(ResourceType.rCameraListXml,
                    ResourceType.rClothXml,
                    ResourceType.rEnemyLayoutXml,
                    ResourceType.rFSMBrainXml,
                    ResourceType.rMarkerLayoutXml,
                    ResourceType.rModelInfoXml,
                    ResourceType.rModelLayoutXml,
                    ResourceType.rNMMachineXml,
                    ResourceType.rRouteNodeXml,
                    ResourceType.rSchedulerXml,
                    ResourceType.rSoundSegXml,
                    ResourceType.rUBCellXml,
                    ResourceType.rEventTimeSchedule,
                    ResourceType.rHavokVehicleData,
                    ResourceType.rAreaHitLayout,
                    ResourceType.rHavokConstraintLayout,
                    ResourceType.rHavokLinkCollisionLayout,
                    ResourceType.rHavokVertexLayout,
                    ResourceType.rItemLayout,
                    ResourceType.rMobLayout,
                    ResourceType.rSprLayout,
                    ResourceType.rSMAdd,
                    ResourceType.rMapLink,
                    ResourceType.rRouteNode,
                    ResourceType.rEnemyLayout,
                    ResourceType.rUBCell,
                    ResourceType.rSoundSeg)]
    public partial class TextEditor : GameResourceEditorControl
    {
        //public XmlFile TextFile { get { return (XmlFile)this.GameResource; } }

        public TextEditor()
        {
            InitializeComponent();
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
            {
                // Clear the textbox contents and return.
                this.textbox.Text = "";
                return;
            }

            // Only allow editing for patch files.
            this.textbox.ReadOnly = !this.ArcFile.IsPatchFile;

            // Get the xml text buffer from the file.
            if (this.GameResource.GetType() == typeof(XmlFile))
                this.textbox.Text = Encoding.Default.GetString(((XmlFile)this.GameResource).Buffer);
            else if (this.GameResource.GetType() == typeof(rItemLayout))
                this.textbox.Text = Encoding.Default.GetString(((rItemLayout)this.GameResource).Buffer);
            else if (this.GameResource.GetType() == typeof(rAreaHitLayout))
                this.textbox.Text = Encoding.Default.GetString(((rAreaHitLayout)this.GameResource).Buffer);
            else if (this.GameResource.GetType() == typeof(BinaryXmlFile))
            {
                // Get the binary xml file and convert the parsed object to a string.
                BinaryXmlFile xmlFile = (BinaryXmlFile)this.GameResource;
                this.textbox.Text = XmlObjectToString(xmlFile.ParsedObject);
            }

            // Reset modification trackers.
            this.HasBeenModified = false;
            this.textbox.IsChanged = false;
        }

        public override bool SaveResource()
        {
            // Set the UI state to disabled while we write to file.
            this.EditorOwner.SetUIState(false);

            // Update the xml file buffer.
            byte[] buffer = Encoding.ASCII.GetBytes(this.textbox.Text);

            // Get a list of duplicate datums that we should update and update all of them.
            DatumIndex[] datums = this.EditorOwner.GetDatumsToUpdateForResource(this.GameResource.FileName);
            if (ArchiveCollection.Instance.InjectFile(datums, buffer) == false)
            {
                // Failed to update files.
                return false;
            }

            // Flag that we no longer have changes made to the resource.
            this.HasBeenModified = false;
            this.textbox.IsChanged = false;

            // Changes saved successfully, re-enable the UI.
            this.EditorOwner.SetUIState(true);
            MessageBox.Show("Changes saved successfully!");
            return true;
        }

        private void textbox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            // Flag that the resource data has been modified.
            this.HasBeenModified = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Check for the Ctrl+S hotkey.
            if (keyData == (Keys.Control | Keys.S))
            {
                // If changes have been made save them.
                if (this.HasBeenModified == true)
                {
                    SaveResource();
                }

                // Skip passing the event to the base class.
                return true;
            }

            // Let the base class handle it.
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string XmlObjectToString(object obj, int tabCount = 0)
        {
            string objStr = "";

            // Get a list of all fields that have the XmlField attribute attached.
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => f.GetCustomAttribute(typeof(XmlFieldAttribute)) != null).ToArray();

            // Loop and print each field.
            for (int i = 0; i < fields.Length; i++)
            {
                // Get the field type.
                string fieldType = fields[i].FieldType.Name;
                if (fieldType.Contains('.') == true)
                    fieldType = fieldType.Substring(fieldType.LastIndexOf('.') + 1);

                // Check the field type and handle accordingly.
                if (fieldType == "Vector3")
                {
                    Vector3 vec = (Vector3)fields[i].GetValue(obj);
                    objStr += new string('\t', tabCount) + string.Format("Vector3 {0}: x={1} y={2} z={3}", fields[i].Name, vec.X, vec.Y, vec.Z) + "\r\n";
                }
                else if (fields[i].FieldType.IsArray == true)
                {
                    // Get the array value from the field.
                    Array array = (Array)fields[i].GetValue(obj);

                    // Print the block start.
                    objStr += new string('\t', tabCount) + fieldType + ": " + fields[i].Name + "\r\n" + new string('\t', tabCount) + "[\r\n";

                    // Loop and print each element.
                    for (int x = 0; x < array.Length; x++)
                    {
                        // Get the array value so we can check its type.
                        object arrayValue = array.GetValue(x);

                        // Check if the array base type is primitive or not.
                        if (arrayValue.GetType().IsPrimitive == true)
                        {
                            // Print the object value.
                            objStr += new string('\t', tabCount + 1) + string.Format("{0}: {1}\r\n", fieldType.Replace("[]", ""), arrayValue);
                        }
                        else
                        {
                            // Print the block start.
                            objStr += new string('\t', tabCount + 1) + "[\r\n";

                            // Print the object fields.
                            objStr += XmlObjectToString(arrayValue, tabCount + 2);

                            // Print the block end.
                            objStr += new string('\t', tabCount + 1) + "]\r\n";
                        }
                    }

                    // Print the block end.
                    objStr += new string('\t', tabCount) + "]\r\n";
                }
                else if (fields[i].FieldType.IsPrimitive == true)
                {
                    objStr += new string('\t', tabCount) + string.Format("{0} {1}: {2}\r\n", fieldType, fields[i].Name, fields[i].GetValue(obj));
                }
                else if (fields[i].FieldType.IsValueType == true)
                {
                    // Print the block start.
                    objStr += new string('\t', tabCount) + fieldType + ": " + fields[i].Name + "\r\n" + new string('\t', tabCount) + "[\r\n";

                    // Print the object fields.
                    objStr += XmlObjectToString(fields[i].GetValue(obj), tabCount + 1);

                    // Print the block end.
                    objStr += new string('\t', tabCount) + "]\r\n";
                }
                else
                {

                }
            }

            // Return the string.
            return objStr;
        }
    }
}
