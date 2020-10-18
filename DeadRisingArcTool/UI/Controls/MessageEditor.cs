using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.Controls;
using DeadRisingArcTool.FileFormats.Misc;

namespace DeadRisingArcTool.UI.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rMessage)]
    public partial class MessageEditor : GameResourceEditorControl
    {
        public MessageEditor()
        {
            InitializeComponent();
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
            {
                // Clear the textbox contents and return.
                this.fastColoredTextBox1.Text = "";
                return;
            }

            // Cast the game resource to a rMessage object.
            rMessage message = (rMessage)this.GameResource;

            // Clear the textbox contents and suspend the UI while we update.
            this.Enabled = false;
            this.fastColoredTextBox1.Text = "";

            // Loop through every string in the message file and decode it.
            string text = "";
            for (int i = 0; i < message.header.StringCount; i++)
            {
                if (i == 285)
                {

                }

                // Decode the string entry and add it to the textbox as its own line.
                text += DecodeMessageEntry(message.strings[i]) + "\r\n";
            }

            // Resume the layout of the text box.
            this.fastColoredTextBox1.Text = text;
            this.Enabled = true;
        }

        public override bool SaveResource()
        {
            return false;
        }

        private string[] TextColors = new string[]
        {
            "Alpha",
            "Red",
            "Green",
            "Blue",
            "Yellow",
            "White",
            "Grey"
        };

        private string DecodeMessageEntry(CharEntry[] chars)
        {
            string str = "";

            // Loop and process each character.
            for (int i = 0; i < chars.Length; i++)
            {
                // Check if the current character is a special character or not.
                if ((chars[i].Flags & 4) != 0)
                {
                    // Check the special case character and handle accordingly.
                    byte specialChar = (byte)chars[i].Character;
                    switch (specialChar)
                    {
                        case 3:
                            {
                                // newline
                                str += "[\\r\\n]";
                                break;
                            }
                        case 26:
                            {
                                // image id (2 characters, 2nd is image id)
                                str += "[Image: " + chars[i + 1].SpriteId.ToString() + "]";
                                i++;
                                break;
                            }
                        case 32:
                            {
                                // Text color (2 characters, second is color code)
                                string color = chars[i + 1].SpriteId - 1 < TextColors.Length ? TextColors[chars[i + 1].SpriteId - 1] : TextColors[6];
                                str += "[Color: " + color + "]";
                                i++;
                                break;
                            }
                        case 33:
                            {
                                // Reset color (1 character)
                                str += "[EndColor]";
                                break;
                            }
                        default:
                            {
                                str += "[SC " + specialChar.ToString() + "]";
                                break;
                            }
                    }
                }
                else
                {
                    // Normal character, add as-is.
                    str += chars[i].Character;
                }
            }

            // Return the decoded string.
            return str;
        }
    }
}
