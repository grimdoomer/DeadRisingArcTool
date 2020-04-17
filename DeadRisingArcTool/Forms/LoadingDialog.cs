using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.Forms
{
    public partial class LoadingDialog : Form
    {
        public LoadingDialog()
        {
            InitializeComponent();
        }

        public void SetupProgress(int max)
        {
            // Set the progress bar maximum.
            this.progressBar1.Maximum = max;
        }

        public void UpdateProgress(string fileName)
        {
            // Update progress.
            this.lblFileName.Text = fileName;
            this.progressBar1.Value++;
        }
    }
}
