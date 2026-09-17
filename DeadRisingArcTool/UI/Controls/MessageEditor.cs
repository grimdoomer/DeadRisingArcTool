using DeadRisingArcTool.Controls;
using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Misc;
using DeadRisingArcTool.FileFormats.Text;
using DeadRisingArcTool.FileFormats.Text.Font;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.UI.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rMessage)]
    public partial class MessageEditor : GameResourceEditorControl
    {
        Messys fontMapping = new Messys();

        public MessageEditor()
        {
            InitializeComponent();

            this.fastColoredTextBox1.BookmarkColor = Color.Red;
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

            // Only allow editing for patch files.
            this.fastColoredTextBox1.ReadOnly = !this.ArcFile.IsPatchFile;

            // Cast the game resource to a rMessage object.
            rMessage message = (rMessage)this.GameResource;

            // Clear the textbox contents and suspend the UI while we update.
            this.Enabled = false;
            this.fastColoredTextBox1.Text = "";

            // Loop through every string in the message file and decode it.
            string text = "";
            for (int i = 0; i < message.header.StringCount; i++)
            {
                // Decode the string entry and add it to the textbox as its own line.
                text += DecodeMessageEntry(message.strings[i]);
                if (i < message.header.StringCount - 1)
                    text += "\r\n";
            }

            // Resume the layout of the text box.
            this.fastColoredTextBox1.Text = text;
            this.Enabled = true;

            // Reset modification trackers.
            this.HasBeenModified = false;
            this.fastColoredTextBox1.IsChanged = false;
        }

        public override bool SaveResource()
        {
            List<int> invalidLines = new List<int>();

            // Set the UI state to disabled while we write to file.
            this.EditorOwner.SetUIState(false);
            this.fastColoredTextBox1.Bookmarks.Clear();

            // Cast the game resource to a rMessage object.
            rMessage message = (rMessage)this.GameResource;

            // Allocate an array to hold the encoded line data.
            int lineCount = this.fastColoredTextBox1.LinesCount;
            CharEntry[][] encodedLines = new CharEntry[lineCount][];

            // Loop and encode lines.
            for (int i = 0; i < lineCount; i++)
            {
                if (EncodeLine(this.fontMapping, this.fastColoredTextBox1.Lines[i], (char)message.header.TerminatorChar, out encodedLines[i]) == false)
                {
                    // Line has errors.
                    this.fastColoredTextBox1.BookmarkLine(i);
                    invalidLines.Add(i);
                }

                //if (encodedLines[i].Length != message.strings[i].Length + 1)
                //{

                //}

                //for (int x = 0; x < encodedLines[i].Length - 1; x++)
                //{
                //    if (encodedLines[i][x].Character != message.strings[i][x].Character ||
                //        encodedLines[i][x].SpriteId != message.strings[i][x].SpriteId ||
                //        encodedLines[i][x].Width != message.strings[i][x].Width ||
                //        encodedLines[i][x].Flags != message.strings[i][x].Flags)
                //    {

                //    }
                //}
            }

            // If we found errors bail out.
            if (invalidLines.Count > 0)
            {
                string lineNumbers = string.Join(", ", invalidLines.ToArray());
                MessageBox.Show("The following lines have errors and need to be fixed. The lines have been highlighted/bookmarked for easy navigation.\n\n" + lineNumbers.Substring(2));

                this.EditorOwner.SetUIState(true);
                return false;
            }

            // Update the rmessage file.
            message.strings = encodedLines;
            byte[] buffer = message.ToBuffer();

            // Get a list of duplicate datums that we should update and update all of them.
            DatumIndex[] datums = this.EditorOwner.GetDatumsToUpdateForResource(this.GameResource.FileName);
            if (ArchiveCollection.Instance.InjectFile(datums, buffer) == false)
            {
                // Failed to update files.
                return false;
            }

            // Flag that we no longer have changes made to the resource.
            this.HasBeenModified = false;
            this.fastColoredTextBox1.IsChanged = false;

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
                //if (this.HasBeenModified == true)
                {
                    SaveResource();
                }

                // Skip passing the event to the base class.
                return true;
            }

            // Let the base class handle it.
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private string[] TextColors = new string[]
        {
            "Black",
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
                if (chars[i].IsSpecialCharacter() == true)
                {
                    // Check the special case character and handle accordingly.
                    rMessageSpecialCharacter specialChar = (rMessageSpecialCharacter)chars[i].Character;
                    switch (specialChar)
                    {
                        case rMessageSpecialCharacter.NewLine:
                            {
                                // newline
                                str += "[\\r\\n]";
                                break;
                            }
                        case rMessageSpecialCharacter.Image:
                            {
                                Debug.Assert(chars[i + 1].Character == 0x11A);

                                // image id (2 characters, 2nd is image id)
                                str += $"[Image: {chars[i + 1].SpriteId}]";
                                i++;
                                break;
                            }
                        case rMessageSpecialCharacter.ColorStart:
                            {
                                Debug.Assert(chars[i + 1].Character == 0x120);

                                // Text color (2 characters, second is color code)
                                string color = chars[i + 1].SpriteId - 1 < TextColors.Length ? TextColors[chars[i + 1].SpriteId - 1] : TextColors[6];
                                str += $"[Color: {color}]";
                                i++;
                                break;
                            }
                        case rMessageSpecialCharacter.ColorEnd:
                            {
                                // Reset color (1 character)
                                str += "[EndColor]";
                                break;
                            }
                        default:
                            {
                                str += $"[SC={specialChar} Flags=0x{chars[i].Flags:X} SpriteId={chars[i].SpriteId}]";
                                break;
                            }
                    }
                }
                else
                {
                    // Check if the character is a square bracket and if so escape it.
                    if (this.fontMapping.CharacterTable.TryGetValue(chars[i].Character, out GameFontSpriteCharacter lookupChar) == true)
                    {
                        if (lookupChar.AsciiCharacter == '[' || lookupChar.AsciiCharacter == ']')
                        {
                            // Escape the characte by doubling it.
                            str += new string(lookupChar.AsciiCharacter, 2);
                        }
                        else
                        {
                            // Normal character, add as-is.
                            str += lookupChar.AsciiCharacter;
                        }
                    }
                    else
                    {
                        // Unsupported char.
                        str += "?";
                    }
                }
            }

            // Return the decoded string.
            return str;
        }

        private bool EncodeLine(GameFontSpriteSheet spriteSheet, string line, char terminatorChar, out CharEntry[] encodedCharacters)
        {
            encodedCharacters = null;

            // Create a list of character entries that represent the string.
            List<CharEntry> characters = new List<CharEntry>();

            // Loop through the string and process.
            for (int i = 0; i < line.Length; i++)
            {
                // Check if the current character is a square bracket.
                if (line[i] == '[' || line[i] == ']')
                {
                    // Check if it's escaped or not.
                    if (i + 1 < line.Length && line[i + 1] == line[i])
                    {
                        // Character is escaped, encode it as-is.
                        GameFontSpriteCharacter charEntry = spriteSheet.CharacterTable[line[i]];
                        characters.Add(new CharEntry(line[i], (short)charEntry.SpriteNumber, (byte)charEntry.Width, 0));
                        i++;
                    }
                    else
                    {
                        // The character is not escaped, parse the special character.
                        string snippet = "";
                        for (int x = i + 1; x < line.Length; x++, i++)
                        {
                            if (line[x] != ']')
                                snippet += line[x];
                            else
                                break;
                        }

                        // Account for ending bracket.
                        i++;

                        // Check if there are additional parameters to parse.
                        int index = snippet.IndexOf(':');
                        if (index != -1)
                        {
                            // Parse the parameter.
                            string parameter = snippet.Substring(0, index);
                            if (parameter == "Color")
                            {
                                // Parse color value.
                                string colorStr = snippet.Substring(index + 1).Trim();
                                if (TextColors.Contains(colorStr) == false)
                                    return false;

                                int colorIndex = Array.IndexOf(TextColors, colorStr) + 1;
                                characters.Add(new CharEntry((char)rMessageSpecialCharacter.ColorStart, 0, 0, 0x4));
                                characters.Add(new CharEntry((char)((short)rMessageSpecialCharacter.ColorStart + 0x100), (short)colorIndex, 0, 0x4));
                            }
                            else if (parameter == "Image")
                            {
                                // Parse the sprite id value.
                                string spriteIdStr = snippet.Substring(index + 1);
                                if (int.TryParse(spriteIdStr, out int spriteId) == false)
                                    return false;

                                characters.Add(new CharEntry((char)rMessageSpecialCharacter.Image, 0, 0, 0x4));
                                characters.Add(new CharEntry((char)((short)rMessageSpecialCharacter.Image + 0x100), (short)spriteId, 0, 0x4));
                            }
                            else
                            {
                                // Unsupported special character.
                                return false;
                            }
                        }
                        else if (snippet == "EndColor")
                        {
                            characters.Add(new CharEntry((char)rMessageSpecialCharacter.ColorEnd, 0, 0, 0x4));
                        }
                        else if (snippet == "\\r\\n")
                        {
                            characters.Add(new CharEntry((char)rMessageSpecialCharacter.NewLine, 0, 0, 0x4));
                        }
                        else
                        {
                            // Try to regex match the snippet.
                            Match regexMatch = Regex.Match(snippet, @"^SC=(?<ordinal>\d+)\s+Flags=0x(?<flags>\S+)\s+SpriteId=(?<spriteId>\d+)");
                            if (regexMatch.Success == true)
                            {
                                // Try to parse parameters.
                                if (int.TryParse(regexMatch.Groups["ordinal"].Value, out int ordinal) == false ||
                                    int.TryParse(regexMatch.Groups["flags"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int flags) == false ||
                                    int.TryParse(regexMatch.Groups["spriteId"].Value, out int spriteId) == false)
                                    return false;

                                characters.Add(new CharEntry((char)ordinal, (short)spriteId, 0, (byte)flags));
                            }
                        }
                    }
                }
                else
                {
                    // Encode normal characters.
                    GameFontSpriteCharacter charEntry = spriteSheet.CharacterTable[line[i]];
                    characters.Add(new CharEntry(line[i], (short)charEntry.SpriteNumber, (byte)charEntry.Width, 0));
                }
            }

            // Add the line terminator.
            characters.Add(new CharEntry(terminatorChar, 0, 0, 0x4));

            // Return the encoded character array.
            encodedCharacters = characters.ToArray();
            return true;
        }
    }
}
