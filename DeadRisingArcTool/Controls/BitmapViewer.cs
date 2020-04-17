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
    public partial class BitmapViewer : UserControl
    {
        // Suspends certain UI operations until the form is fully initialized.
        private bool isLoading = false;

        private rTexture bitmap = null;
        /// <summary>
        /// Gets or sets the rTexture bitmap currently being displayed.
        /// </summary>
        public rTexture Bitmap
        {
            get { return this.bitmap; }
            set { this.bitmap = value; ReloadBitmap(); }
        }

        public BitmapViewer()
        {
            InitializeComponent();
        }

        public BitmapViewer(rTexture bitmap)
        {
            // Initialize fields.
            this.bitmap = bitmap;

            InitializeComponent();
        }

        private void BitmapViewer_Load(object sender, EventArgs e)
        {
            // If the control was created with a bitmap image set, reload the UI.
            if (this.bitmap != null)
                ReloadBitmap();
        }

        private void ReloadBitmap()
        {
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
            this.pictureBox1.Image = this.Bitmap.GetBitmap(0);
            if (this.pictureBox1.Image != null)
                this.pictureBox1.Size = this.pictureBox1.Image.Size;

            // Check if we need to set the background color.
            if ((this.bitmap.header.Flags & 4) != 0)
            {
                // Convert the background color to integers.
                int b = 255 * (int)this.bitmap.BackgroundColor[0];
                int g = 255 * (int)this.bitmap.BackgroundColor[1];
                int r = 255 * (int)this.bitmap.BackgroundColor[2];
                int a = 255 * (int)this.bitmap.BackgroundColor[3];

                // Set the background color to the inverse of the values?
                //this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(255 - a, 255 - r, 255 - g, 255 - b);
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
            this.pictureBox1.Image = this.Bitmap.GetBitmap(this.comboBox1.SelectedIndex);
        }
    }
}
