using System;
using System.Collections.Generic;
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
            bool createdNew;
            Mutex mutex = new Mutex(true, "Captures", out createdNew);
            if (!createdNew) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Menu());
            //--------------------------------
        }
        //=================================================================
    }
}
