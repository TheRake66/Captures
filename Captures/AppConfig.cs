using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captures
{
    public class AppConfig
    {
        //=================================================================
        public CultureInfo Language     = new CultureInfo("fr");
        public bool StartOnBoot         = false;
        public bool Notify              = true;
        public int WindowSizeWidth      = 700;
        public int WindowSizeHeight     = 600;
        public int IconSize             = 64;
        public ImageFormat Format       = ImageFormat.Png;
        public int KeyAllsScreens       = 9;
        public int KeyActiveWindow      = 8;
        public int KeyZone              = 10;
        public string SavePathFolder    = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\Captures";
        //=================================================================
    }
}
