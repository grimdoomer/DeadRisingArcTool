using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats.Geometry;
using System.Reflection;
using DeadRisingArcTool.Utilities;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rMotionList)]
    public partial class AnimationEditor : GameResourceEditorControl
    {
        public AnimationEditor()
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

            // Cast the resource to an rMotionList object.
            rMotionList motion = (rMotionList)this.GameResource;

            // Print all the animation information out to the textbox.
            string headerInfo = string.Format("*** Header\n{0}\n", StructureToString(motion.header));

            string animationDesc = "";
            for (int i = 0; i < motion.header.AnimationCount; i++)
            {
                animationDesc += string.Format("****************** Animation #{0} ******************\n{1}\n", i, StructureToString(motion.animations[i]));
                
                for (int x = 0; x < motion.animations[i].JointCount; x++)
                {
                    animationDesc += string.Format("\tKey Frame {0}:\tBufferType={1}\tUsage={2}\t\tJointType={3}\tJointIndex={4}\tWeight={5}\t\tDataSize={6}\tDataOffset={7}\n",
                        x, motion.animations[i].KeyFrames[x].Codec, motion.animations[i].KeyFrames[x].Usage, motion.animations[i].KeyFrames[x].JointType,
                        motion.animations[i].KeyFrames[x].JointIndex, motion.animations[i].KeyFrames[x].BlendWeight, motion.animations[i].KeyFrames[x].DataSize,
                        motion.animations[i].KeyFrames[x].DataOffset);

                    //for (int z = 0; z < motion.animations[i].KeyFrames[x].KeyFrameData.Length; z++)
                    //{
                    //    animationDesc += string.Format("\t\tComponent= X={0}\tY={1}\tZ={2}\t\tFlags={3}\n", motion.animations[i].KeyFrames[x].KeyFrameData[z].Component.X,
                    //        motion.animations[i].KeyFrames[x].KeyFrameData[z].Component.Y, motion.animations[i].KeyFrames[x].KeyFrameData[z].Component.Z,
                    //        motion.animations[i].KeyFrames[x].KeyFrameData[z].Flags);
                    //}
                }

                animationDesc += "\n";
            }

            this.textBox1.Text = headerInfo.Replace("\n", "\r\n") + animationDesc.Replace("\n", "\r\n");
        }

        public override bool SaveResource()
        {
            throw new NotImplementedException();
        }

        private string StructureToString(object obj)
        {
            string outputString = "";

            // Get a list of all fields for the structure and format them all into a string.
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                // Get the value of field and make sure it's not null.
                object value = fields[i].GetValue(obj);
                if (value != null)
                {
                    // Check if the field has a hex attribute on it.
                    if (fields[i].GetCustomAttribute<HexAttribute>() != null)
                        outputString += string.Format("\t{0}:{1}\n", fields[i].Name, int.Parse(value.ToString(), System.Globalization.NumberStyles.Integer).ToString("X"));
                    else
                        outputString += string.Format("\t{0}:{1}\n", fields[i].Name, value.ToString());
                }
            }

            // Return the string.
            return outputString;
        }
    }
}
