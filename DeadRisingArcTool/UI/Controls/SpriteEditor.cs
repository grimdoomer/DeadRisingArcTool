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
using System.Reflection;
using DeadRisingArcTool.Utilities;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Archive;
using System.Drawing.Imaging;

namespace DeadRisingArcTool.UI.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rSprAnm)]
    public partial class SpriteEditor : GameResourceEditorControl
    {
        /// <summary>
        /// Preferred size of the picture box based on the size of the bitmap viewer control.
        /// </summary>
        private Size PreferredImageSize
        {
            get { return new Size(this.Size.Width - 8, this.Size.Height - this.pictureBox1.Location.Y - 3); }
        }

        private int spriteSetIndex = 0;
        private int spriteIndex = 0;

        private rTexture textureFile = null;
        private Bitmap texture = null;

        private bool isUpdating = false;

        public SpriteEditor()
        {
            InitializeComponent();
        }

        private void ClearUI()
        {
            // Clear the textbox contents and return.
            this.cmbSpriteSet.Items.Clear();
            this.cmbSprite.Items.Clear();
            this.textBox1.Text = "";
            this.pictureBox1.Image = null;
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
            {
                // Clear the textbox contents and return.
                ClearUI();
                return;
            }

            // Cast the game resource to a rSprAnm object.
            rSprAnm sprite = (rSprAnm)this.GameResource;

            // Make sure there is at least one sprite set.
            if (sprite.blitInfo.Length == 0)
            {
                // Clear the UI and return.
                ClearUI();
                return;
            }

            // Check if there is a texture file with the same name.
            ArchiveCollection.Instance.GetArchiveFileEntryFromFileName(this.GameResource.FileName.Replace(".rSprAnm", ".rTexture"), out Archive archive, out ArchiveFileEntry fileEntry);
            if (fileEntry != null)
            {
                // Load the texture file.
                this.textureFile = ArchiveCollection.Instance.GetFileAsResource<rTexture>(new DatumIndex(archive.ArchiveId, fileEntry.FileId));

                // Get the default LOD bitmap.
                this.texture = this.textureFile.GetBitmap(0);
            }
            else
            {
                this.textureFile = null;
                this.texture = null;
            }

            // Flag that we are updating so the combo boxes don't update.
            this.isUpdating = true;

            // Add items to the sprite set combo box.
            this.cmbSpriteSet.Items.Clear();
            for (int i = 0; i < sprite.blitInfo.Length; i++)
                this.cmbSpriteSet.Items.Add(i.ToString());

            // Add items to the sprite index combo box.
            this.cmbSprite.Items.Clear();
            for (int i = 0; i < sprite.blitInfo[0].Length; i++)
                this.cmbSprite.Items.Add(i.ToString());

            // Flag that we are no longer updating.
            this.isUpdating = false;

            // Manully trigger the sprite set index changed event.
            this.cmbSpriteSet.SelectedIndex = 0;
        }

        public override bool SaveResource()
        {
            throw new NotImplementedException();
        }

        private void CmbSpriteSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if the form is updating and if so bail out.
            if (this.isUpdating == true)
                return;

            // Get the game resource as a rSprAnm object.
            rSprAnm sprite = (rSprAnm)this.GameResource;

            // Flag that we are updating so we don't trigger events.
            this.isUpdating = true;

            // Set the sprite set index.
            this.spriteSetIndex = this.cmbSpriteSet.SelectedIndex;

            // Reset items in the sprite index combo box.
            this.cmbSprite.Items.Clear();
            for (int i = 0; i < sprite.blitInfo[this.spriteSetIndex].Length; i++)
                this.cmbSprite.Items.Add(i.ToString());

            // Flag that we are no longer updating.
            this.isUpdating = false;

            // Manually trigger the sprite combo box index changed event.
            this.cmbSprite.SelectedIndex = 0;
        }

        private void CmbSprite_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if the form is updating and if so bail out.
            if (this.isUpdating == true)
                return;

            // Get the game resource as a rSprAnm object.
            rSprAnm sprite = (rSprAnm)this.GameResource;

            // Set the sprite index and update the textbox text.
            this.spriteIndex = this.cmbSprite.SelectedIndex;
            this.textBox1.Text = StructureToString(sprite.blitInfo[this.spriteSetIndex][this.spriteIndex]).Replace("\n", "\r\n");

            // If there is a loaded bitmap create a preview image for the sprite.
            if (this.textureFile != null)
            {
                // Create a new bitmap for the sprite subsection.
                SpriteBlitInfo blitInfo = sprite.blitInfo[this.spriteSetIndex][this.spriteIndex];
                Bitmap spriteBitmap = new Bitmap(blitInfo.Width, blitInfo.Height, this.texture.PixelFormat);

                // Compute the x and y position of the sprite.
                int xpos = ((blitInfo.XRunCount * 256) + blitInfo.PosX) * 4;
                int ypos = (blitInfo.YRunCount * 256) + blitInfo.PosY;

                // Lock the bitmaps so we can copy the pixels.
                BitmapData spriteBitmData = spriteBitmap.LockBits(new Rectangle(0, 0, spriteBitmap.Width, spriteBitmap.Height), ImageLockMode.ReadWrite, spriteBitmap.PixelFormat);
                BitmapData textureBitmData = this.texture.LockBits(new Rectangle(0, 0, this.texture.Width, this.texture.Height), ImageLockMode.ReadOnly, this.texture.PixelFormat);

                unsafe
                {
                    byte* src = (byte*)textureBitmData.Scan0.ToPointer();
                    byte* dst = (byte*)spriteBitmData.Scan0.ToPointer();

                    for (int y = 0; y < blitInfo.Height; y++)
                    {
                        for (int x = 0; x < spriteBitmData.Stride; x++)
                            dst[(y * spriteBitmData.Stride) + x] = src[((ypos + y) * textureBitmData.Stride) + xpos + x];
                    }
                }

                // Unlock the pixel buffers.
                spriteBitmap.UnlockBits(spriteBitmData);
                this.texture.UnlockBits(textureBitmData);

                // Set the sprite image.
                this.pictureBox1.Image = spriteBitmap;

                // If the image is larger than the picture box then scale it.
                if (spriteBitmap.Width > this.PreferredImageSize.Width || spriteBitmap.Height > this.PreferredImageSize.Height)
                {
                    // Set the picture box to the preferred size and scale the image to fit.
                    this.pictureBox1.Size = this.PreferredImageSize;
                    this.pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    // Set the picture box to be the same size as the image.
                    this.pictureBox1.Size = spriteBitmap.Size;
                    this.pictureBox1.BackgroundImageLayout = ImageLayout.None;
                }
            }
            else
                this.pictureBox1.Image = null;
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
                        outputString += string.Format("{0}: {1}\n", fields[i].Name, int.Parse(value.ToString(), System.Globalization.NumberStyles.Integer).ToString("X"));
                    else
                        outputString += string.Format("{0}: {1}\n", fields[i].Name, value.ToString());
                }
            }

            // Return the string.
            return outputString;
        }
    }
}
