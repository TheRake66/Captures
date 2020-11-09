using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Captures
{
    static class Program
    {
        //=================================================================
        [STAThread]
        static void Main()
        {
            //--------------------------------
            try
            {
                Thread.Sleep(500); // Pour le restart

                int count = 0;
                Process[] processCollection = Process.GetProcesses();
                foreach (Process p in processCollection)
                {
                    if (p.ProcessName == Path.GetFileNameWithoutExtension(Application.ExecutablePath)) count++;
                }

                // Une seule instance
                if (count == 1)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Menu());
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show(MenuLang.Fatal_Error + ex.Message, "Captures", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //--------------------------------
        }
        //=================================================================
    }
}
