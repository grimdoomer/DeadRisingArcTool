using DeadRisingArcTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // If the update flavor is not set then set it to major release builds.
            if (string.IsNullOrEmpty(Properties.Settings.Default.UpdateFlavor) == true)
            {
                // Set the update flavor and save the settings file.
                Properties.Settings.Default.UpdateFlavor = UpdateFlavor.Release.ToString();
                Properties.Settings.Default.Save();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
