using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats.Bitmaps;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(FileFormats.ResourceType.rTexture)]
    public partial class BitmapViewer : GameResourceEditorControl
    {
        /// <summary>
        /// Private accessor that casts the game resource to a <see cref="rTexture"/>
        /// </summary>
        private rTexture Bitmap { get { return (rTexture)this.GameResource; } }

        // Suspends certain UI operations until the form is fully initialized.
        private bool isLoading = false;

        /// <summary>
        /// Preferred size of the picture box based on the size of the bitmap viewer control.
        /// </summary>
        private Size PreferredImageSize
        {
            get { return new Size(this.Size.Width - 8, this.Size.Height - this.pictureBox1.Location.Y - 3); }
        }

        public BitmapViewer()
        {
            InitializeComponent();
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
                return;

            // Flag that we are reloading the UI.
            this.isLoading = true;

            // Clear the combobox items.
            this.comboBox1.Items.Clear();

            // Populate the bitmap info fields.
            this.lblWidth.Text = this.Bitmap.header.Width.ToString();
            this.lblHeight.Text = this.Bitmap.header.Height.ToString();
            this.lblFormat.Text = this.Bitmap.header.Format.ToString().Replace("Format_", "");
            this.lblType.Text = this.Bitmap.header.TextureType.ToString().Replace("Type_", "");
            this.lblFlags.Text = this.Bitmap.header.Flags.ToString();
            this.lblMipMaps.Text = this.Bitmap.header.MipMapCount.ToString();
            this.lblDepth.Text = this.Bitmap.header.Depth.ToString();

            // Populate the combobox.
            for (int i = 0; i < this.Bitmap.header.MipMapCount; i++)
                this.comboBox1.Items.Add(i.ToString());

            // Select the first mip map level.
            this.comboBox1.SelectedIndex = 0;

            // Load the first mip map image into the preview box.
            Bitmap bitmap = this.Bitmap.GetBitmap(0);
            this.pictureBox1.BackgroundImage = bitmap;
            if (bitmap == null)
            {
                // Flag that we are no longer loading the form and bail.
                this.isLoading = false;
                return;
            }


            // If the image is larger than the picture box then scale it.
            if (bitmap.Width > this.PreferredImageSize.Width || bitmap.Height > this.PreferredImageSize.Height)
            {
                // Set the picture box to the preferred size and scale the image to fit.
                this.pictureBox1.Size = this.PreferredImageSize;
                this.pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                // Set the picture box to be the same size as the image.
                this.pictureBox1.Size = bitmap.Size;
                this.pictureBox1.BackgroundImageLayout = ImageLayout.None;
            }

            // Check if we need to set the background color.
            if (this.Bitmap.header.Flags.HasFlag(TextureFlags.HasD3DClearColor) == true)
            {
                // Convert the background color to integers (RGBA order?).
                int b = 255 * (int)this.Bitmap.BackgroundColor[0];
                int g = 255 * (int)this.Bitmap.BackgroundColor[1];
                int r = 255 * (int)this.Bitmap.BackgroundColor[2];
                int a = 255 * (int)this.Bitmap.BackgroundColor[3];

                // Set the background color to the inverse of the values?
                this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(255 - a, 255 - r, 255 - g, 255 - b);
            }
            else
            {
                // Set the background color to default.
                this.pictureBox1.BackColor = System.Drawing.Color.FromKnownColor(KnownColor.Control);
            }

            // Flag that we are no longer loading the form.
            this.isLoading = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if the event was triggered from the UI loading.
            if (this.isLoading == true)
                return;

            // Set the preview image to the selected mip map level.
            this.pictureBox1.BackgroundImage = this.Bitmap.GetBitmap(this.comboBox1.SelectedIndex);
            if (this.pictureBox1.BackgroundImage != null)
            {
                // If the image is larger than the picture box then scale it.
                if (this.pictureBox1.BackgroundImage.Width > this.PreferredImageSize.Width || this.pictureBox1.BackgroundImage.Height > this.PreferredImageSize.Height)
                {
                    // Set the picture box to the preferred size and scale the image to fit.
                    this.pictureBox1.Size = this.PreferredImageSize;
                    this.pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    // Set the picture box to be the same size as the image.
                    this.pictureBox1.Size = this.pictureBox1.BackgroundImage.Size;
                    this.pictureBox1.BackgroundImageLayout = ImageLayout.None;
                }
            }
        }
    }
}
