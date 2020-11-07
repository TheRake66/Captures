using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Captures
{
    public partial class Menu : Form
    {
        //=================================================================
        private const int TAILLE_RED_BORDER = 2;

        // Récupère le handle de la fenetre active
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        // Recupere en reference le rectangle gdi de la fenetre (Sous Windows 7)
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr pHandle, ref Rectangle pRect);
        // Recupere en reference le rectangle gdi de la fenetre (Sous Windows 10)
        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmGetWindowAttribute(IntPtr pHandle, uint pDWAttr, ref Rectangle pRect, int pCBAttrib);
        // Recupere en async les touches pressées
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Touches sélectionnées
        private ToolStripMenuItem SelectedFormat;
        private ToolStripMenuItem SelectedKeyAlls;
        private ToolStripMenuItem SelectedKeyWin;
        private ToolStripMenuItem SelectedKeyZone;

        private bool DrawingCourse; // Pour la zone
        private Point DrawingStartPoint; // idem
        private int TailleImageList; // Taille des image dans l'explorateur
        private string DossierCapture;

        private Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Fichier de config
        //=================================================================



        //=================================================================
        // Démarrage, config + init
        public Menu()
        {
            //--------------------------------
            // Charge les composants
            // Gère la langue
            string langue = this.config.AppSettings.Settings["Language"].Value;

            if (!new string[] { "fr", "es", "en", "zh"}.Contains(langue)) langue = "fr";
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langue);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(langue);

            InitializeComponent();

            switch (langue)
            {
                case "fr":
                    this.françaisToolStripMenuItem.CheckState = CheckState.Indeterminate;
                    break;
                case "en":
                    this.anglaisToolStripMenuItem.CheckState = CheckState.Indeterminate;
                    break;
                case "es":
                    this.espagnolToolStripMenuItem.CheckState = CheckState.Indeterminate;
                    break;
                case "zh":
                    this.chinoisToolStripMenuItem.CheckState = CheckState.Indeterminate;
                    break;
            }
            //--------------------------------
        }
        // Charge les parametres
        private void Menu_Load(object sender, EventArgs e)
        {
            //--------------------------------
            // Charge la config
            string startup      = this.config.AppSettings.Settings["StartOnBoot"].Value;
            string notify       = this.config.AppSettings.Settings["Notify"].Value;
            string sizew        = this.config.AppSettings.Settings["WindowSizeWidth"].Value;
            string sizeh        = this.config.AppSettings.Settings["WindowSizeHeight"].Value;
            string iconsize     = this.config.AppSettings.Settings["IconSize"].Value;
            string formatfile   = this.config.AppSettings.Settings["Format"].Value;
            string keyall       = this.config.AppSettings.Settings["KeyAllsScreens"].Value;
            string keyactive    = this.config.AppSettings.Settings["KeyActiveWindow"].Value;
            string keyzone      = this.config.AppSettings.Settings["KeyZone"].Value;
            string folder       = this.config.AppSettings.Settings["SavePathFolder"].Value;


            // Gère l'autostart
            if (startup.Equals("True")) this.éxecuterAuDémarrageDeWindowsToolStripMenuItem.Checked = true;


            // Gère les notifications
            if (notify.Equals("True")) this.notificationsLorsDesCapturesToolStripMenuItem.Checked = true;


            // Largeur
            try { this.Width = Convert.ToInt32(sizew); }
            catch { this.Width = 700; }


            // Hauteur
            try { this.Height = Convert.ToInt32(sizeh); }
            catch { this.Height = 600; }


            // Taille des images
            try
            {
                this.TailleImageList = Convert.ToInt32(iconsize);
                if (!new int[] { 16, 32, 64, 128, 256}.Contains(this.TailleImageList)) this.TailleImageList = 64;
            }
            catch { this.TailleImageList = 64; }
            this.toolStripStatusLabel2.Text = this.TailleImageList + "x" + this.TailleImageList;


            // Dossier
            string defaultfolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\Czzaptures";
            if (folder.Equals("")) folder = defaultfolder;
            if (!Directory.Exists(folder))
            {
                try { Directory.CreateDirectory(folder); }
                catch
                {
                    try
                    {
                        Directory.CreateDirectory(defaultfolder);
                        folder = defaultfolder;
                    }
                    catch { folder = MenuLang.Error_NoFolder; }
                }
            }
            this.toolStripStatusLabel1.Text = folder;
            this.DossierCapture = folder;


            // Format de fichier
            List<string> format = new List<string>();
            format.Add(".PNG");
            format.Add(".JPEG");
            format.Add(".JPG");
            format.Add(".BMP");
            format.Add(".ICO");
            format.Add(".GIF");
            if (!format.Contains(formatfile)) formatfile = ".PNG";
            this.SelectedFormat = LoadToolStripShortcut(this.formatToolStripMenuItem, format, formatfile);


            // Hotkeys
            List<string> hotkey = new List<string>();
            for (int i = 1; i < 13; i++) { hotkey.Add("F" + i); }

            if (!hotkey.Contains(keyall)) { keyall = "F9"; }
            if (!hotkey.Contains(keyactive)) { keyactive = "F8"; }
            if (!hotkey.Contains(keyzone)) { keyzone = "F10"; }

            bool finderror = false;
            do
            {
                finderror = false;
                if (keyall == keyactive || keyall == keyzone)
                { 
                    keyall = "F9";
                    finderror = true;
                }
                if (keyactive == keyall || keyactive == keyzone)
                {
                    keyactive = "F8";
                    finderror = true;
                }
                if (keyzone == keyall || keyzone == keyactive)
                { 
                    keyzone = "F10";
                    finderror = true;
                }
            } while (finderror);

            this.SelectedKeyAlls = LoadToolStripShortcut(this.touchePourTousToolStripMenuItem, hotkey, keyall);
            this.SelectedKeyWin = LoadToolStripShortcut(this.touchePourActiveToolStripMenuItem, hotkey, keyactive);
            this.SelectedKeyZone = LoadToolStripShortcut(this.touchePourZoneToolStripMenuItem, hotkey, keyzone);


            // Charge les images dans l'explorateur
            LoadCaptureExplorer();
            LoadNotifyToolStrip();
            //--------------------------------
        }
        // Autostart
        private void Menu_Shown(object sender, EventArgs e)
        {
            //--------------------------------
            // Autostart
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1].Equals("--hide")) this.Hide();

            this.Refresh();
            checkUpdate(true);
            //--------------------------------
        }
        // Fermeture
        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            //--------------------------------
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                try { this.config.Save(ConfigurationSaveMode.Modified); }
                catch { }
            }
            //--------------------------------
        }
        // Change le bouton avec la flèche
        private void Menu_Resize(object sender, EventArgs e)
        {
            //--------------------------------
            if (this.Height == this.MinimumSize.Height) this.toolStripStatusLabel4.Image = Properties.Resources.DownIcon;
            else this.toolStripStatusLabel4.Image = Properties.Resources.UpIcon;
            //--------------------------------
        }
        // Sauvegarde la dernière taille
        private void Menu_ResizeEnd(object sender, EventArgs e)
        {
            //--------------------------------
            if (this.WindowState != FormWindowState.Maximized)
            {
                this.config.AppSettings.Settings["WindowSizeWidth"].Value = this.Width.ToString();
                this.config.AppSettings.Settings["WindowSizeHeight"].Value = this.Height.ToString();
            }
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Crée dynamiquement les toolstrip
        private ToolStripMenuItem LoadToolStripShortcut(ToolStripMenuItem pMenuStrip, List<string> pListItem, string pDefault)
        {
            //--------------------------------
            ToolStripMenuItem selected = new ToolStripMenuItem();

            foreach (string itemtext in pListItem)
            {
                ToolStripMenuItem key = new ToolStripMenuItem();
                key.Text = itemtext;
                key.Click += new EventHandler((s, e) => { ClickToolStripShortcut(key); });
                if (itemtext == pDefault)
                { 
                    key.CheckState = CheckState.Indeterminate;
                    selected = key;
                }
                pMenuStrip.DropDownItems.Add(key);
            }

            return selected;
            //--------------------------------
        }
        // Gère la sauvegarde des toolstrip
        private void ClickToolStripShortcut(ToolStripMenuItem pMenuCliked)
        {
            //--------------------------------
            ToolStripItem owner = pMenuCliked.OwnerItem;

            if (owner == this.formatToolStripMenuItem)
            {
                configSave(this.SelectedFormat, pMenuCliked, "Format");
                LoadCaptureExplorer();
            }
            // Tous les écrans
            else if (owner == this.touchePourTousToolStripMenuItem)
            { 
                if (pMenuCliked == this.SelectedKeyWin)       MessageBox.Show(MenuLang.Error_UsedWin, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (pMenuCliked == this.SelectedKeyZone) MessageBox.Show(MenuLang.Error_UsedArea, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    configSave(this.SelectedKeyAlls, pMenuCliked, "KeyAllsScreens");
                    LoadNotifyToolStrip();
                    IsPressed(pMenuCliked); // vide la mémoire
                }
            }
            // Fenetre active
            else if (owner == this.touchePourActiveToolStripMenuItem)
            {
                if (pMenuCliked == this.SelectedKeyAlls)      MessageBox.Show(MenuLang.Error_UsedAlls, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (pMenuCliked == this.SelectedKeyZone) MessageBox.Show(MenuLang.Error_UsedArea, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    configSave(this.SelectedKeyWin, pMenuCliked, "KeyActiveWindow");
                    LoadNotifyToolStrip();
                    IsPressed(pMenuCliked); // vide la mémoire
                }
            }
            // Zone
            else if (owner == this.touchePourZoneToolStripMenuItem)
            {
                if (pMenuCliked == this.SelectedKeyWin)       MessageBox.Show(MenuLang.Error_UsedWin, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (pMenuCliked == this.SelectedKeyAlls) MessageBox.Show(MenuLang.Error_UsedAlls, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    configSave(this.SelectedKeyZone, pMenuCliked, "KeyZone");
                    LoadNotifyToolStrip();
                    IsPressed(pMenuCliked); // vide la mémoire
                }
            }

            /* Pourquoi vider la mémoire ? Lors de l'appel de asynckey il va résupérer la touche cliqué.
             * Exemple :
             * Si pour la caputre complète on a la touche F8, qu'on appuie sur F7 il ne se passe rien (normal)
             * mais si on change la touche pour F7 asynckey va se souvenir qu'on a appuiyé et donc va déclenché l'action.
             * On peut passer asynckey en temps réel mais le timer est de 100ms donc si on appuie pas pile au bon moment
             * ça ne déclenche pas.
             * Er réduire le timer consomme de la RAM */
            //--------------------------------
        }
        // Sauvegarde les toolstrip dans la config et code/decoche
        private void configSave(ToolStripMenuItem aDecocher, ToolStripMenuItem aCocher, string configName)
        {
            //--------------------------------
            aDecocher.Checked = false;
            aCocher.CheckState = CheckState.Indeterminate;
            aDecocher = aCocher;

            this.config.AppSettings.Settings[configName].Value = aCocher.Text;
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Affiche la fenetre en cas de click sur l'icone de notif
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //--------------------------------
            if (e.Button == MouseButtons.Left) AfficherCaptures();
            //--------------------------------
        }
        // Créer le menu toolstrip des notifs
        private void LoadNotifyToolStrip()
        {
            //--------------------------------
            if (this.notifyIcon1.ContextMenuStrip != null) this.notifyIcon1.ContextMenuStrip.Items.Clear(); // First start
            this.notifyIcon1.ContextMenuStrip = new ContextMenuStrip();

            // Touche afficher
            this.notifyIcon1.ContextMenuStrip.Items.Add(MenuLang.Context_Show, null, new EventHandler((s, e) => { AfficherCaptures(); }));
            this.notifyIcon1.ContextMenuStrip.Items.Add("-");

            // Touche tous
            ToolStripMenuItem alls = new ToolStripMenuItem();
            alls.Text = this.toolStripButton1.Text;
            alls.ShortcutKeyDisplayString = this.SelectedKeyAlls.Text;
            alls.Image = this.toolStripButton1.Image;
            alls.Click += new EventHandler((s, e) => { CaptureAlls(false); });
            this.notifyIcon1.ContextMenuStrip.Items.Add(alls);

            // Touche fenetre
            ToolStripMenuItem active = new ToolStripMenuItem();
            active.Text = this.toolStripButton3.Text;
            active.ShortcutKeyDisplayString = this.SelectedKeyWin.Text;
            active.Image = this.toolStripButton3.Image;
            active.Click += new EventHandler((s, e) => { CaptureWin(false); });
            this.notifyIcon1.ContextMenuStrip.Items.Add(active);

            // Touche zone
            ToolStripMenuItem zone = new ToolStripMenuItem();
            zone.Text = this.toolStripButton2.Text;
            zone.ShortcutKeyDisplayString = this.SelectedKeyZone.Text;
            zone.Image = this.toolStripButton2.Image;
            zone.Click += new EventHandler((s, e) => { CaptureZone(false); });
            this.notifyIcon1.ContextMenuStrip.Items.Add(zone);
            this.notifyIcon1.ContextMenuStrip.Items.Add("-");

            // Touche pour infos
            this.notifyIcon1.ContextMenuStrip.Items.Add(this.àProposToolStripMenuItem.Text, null, new EventHandler((s, e) => { AfficherAPropos(); }));
            this.notifyIcon1.ContextMenuStrip.Items.Add("-");

            // Touche quitter
            ToolStripMenuItem quit = new ToolStripMenuItem();
            quit.Text = this.quitterToolStripMenuItem.Text;
            quit.Click += new EventHandler((s, e) => { Application.Exit(); });
            this.notifyIcon1.ContextMenuStrip.Items.Add(quit);
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Ouvre l'image cliquée
        private void listView1_DoubleClick(object sender, EventArgs e) // 
        {
            //--------------------------------
            try {  Process.Start("explorer.exe", this.listView1.SelectedItems[0].Tag.ToString()); }
            catch { MessageBox.Show(MenuLang.Error_AccessFile, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            //--------------------------------
        }
        // Charge les fichiers dans l'explorateur
        private void LoadCaptureExplorer(string pNewFile = "") 
        {
            //--------------------------------
            // Verifi le dossier
            string folder = this.toolStripStatusLabel1.Text;

            if (!Directory.Exists(folder)) MessageBox.Show(MenuLang.Error_FindFolder, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                // Change et rafraichi le label
                this.listView1.Clear(); // Vide l'explorateur
                this.toolStripStatusLabel1.Text = MenuLang.Loading + "...";
                this.toolStripStatusLabel1.IsLink = false;
                this.statusStrip1.Refresh();

                // Ajoute les fichiers dans l'explorateur
                int count = 0; // Index des images
                string format = this.SelectedFormat.Text; // Format de fichier

                ImageList list = new ImageList();
                list.ImageSize = new Size(this.TailleImageList, this.TailleImageList);
                this.listView1.LargeImageList = list;

                foreach (string file in Directory.GetFiles(folder))
                {
                    if (Path.GetExtension(file).ToUpper().Equals(format)) // Bon format
                    {
                        string filename = Path.GetFileName(file);

                        // Ajoute l'image à la liste
                        using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            try { list.Images.Add(Image.FromStream(stream)); }
                            catch
                            {
                                list.Images.Add(new Bitmap(Properties.Resources.ErrorIcon));
                                filename = "[" + MenuLang.Error + "] " + filename;
                            }
                        }

                        if (file == pNewFile) filename = "[" + MenuLang.New + "] " + filename;

                        // Ajoute le fichier à l'explorateur
                        this.listView1.Items.Add(new ListViewItem
                        {
                            ImageIndex = count,
                            Text = filename,
                            Tag = file
                        });
                        count++;
                    }
                }

                this.toolStripStatusLabel9.Text = this.listView1.Items.Count + " " + (this.listView1.Items.Count > 1 ? MenuLang.ElementP : MenuLang.Element);
                this.toolStripStatusLabel1.IsLink = true;
                this.toolStripStatusLabel1.Text = folder; // remet le dossier
            }
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Bouton avec la flèche
        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {
            //--------------------------------
            if (this.Height == this.MinimumSize.Height) this.Height = 600;
            else this.Size = this.MinimumSize;
            //--------------------------------
        }


        // Bouton plus
        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {
            //--------------------------------
            if (this.TailleImageList < 256) refreshStripSize(this.TailleImageList *= 2);
            //--------------------------------
        }
        // Bouton moins
        private void toolStripStatusLabel5_Click(object sender, EventArgs e)
        {
            //--------------------------------
            if (this.TailleImageList > 16) refreshStripSize(this.TailleImageList /= 2);
            //--------------------------------
        }
        // Actualise la taille
        private void refreshStripSize(int size)
        {
            //--------------------------------
            this.TailleImageList = size;
            this.toolStripStatusLabel2.Text = this.TailleImageList + "x" + this.TailleImageList;
            if (this.TailleImageList == 256) this.toolStripStatusLabel2.Text += " (" + MenuLang.Max + ")";
            else if (this.TailleImageList == 16) this.toolStripStatusLabel2.Text += " (" + MenuLang.Min + ")";
            this.config.AppSettings.Settings["IconSize"].Value = this.TailleImageList.ToString();
            LoadCaptureExplorer();
            //--------------------------------
        }
        // Bouton refresh
        private void toolStripStatusLabel6_Click(object sender, EventArgs e)
        {
            //--------------------------------
            LoadCaptureExplorer();
            //--------------------------------
        }


        // Change le label element
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //--------------------------------
            this.toolStripStatusLabel10.Text = "(" + this.listView1.SelectedIndices.Count + " " + (this.listView1.SelectedIndices.Count > 1 ? MenuLang.SelectedP : MenuLang.Selected) + ")";
            //--------------------------------
        }
        // Ouvre les images séléctionnées
        private void toolStripStatusLabel8_Click(object sender, EventArgs e)
        {
            //--------------------------------
            for (int i = 0; i < this.listView1.SelectedItems.Count; i++)
            {
                try { Process.Start("explorer.exe", this.listView1.SelectedItems[i].Tag.ToString()); }
                catch { MessageBox.Show(MenuLang.Error_AccessFile.Replace("{Name}", this.listView1.SelectedItems[i].Text), "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                Thread.Sleep(1000);
            }
            //--------------------------------

        }
        // Supprime les images séléctionnées
        private void toolStripStatusLabel11_Click(object sender, EventArgs e)
        {
            //--------------------------------
            int deleted = 0;
            for (int i = 0; i < this.listView1.SelectedItems.Count; i++)
            {
                try
                {
                    File.Delete(this.listView1.SelectedItems[i].Tag.ToString());
                    deleted++;
                }
                catch { MessageBox.Show(MenuLang.Error_DelFile.Replace("{Name}", this.listView1.SelectedItems[i].Text), "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }

            if (deleted > 0) LoadCaptureExplorer("");
            //--------------------------------
        }


        // Ouvre le dossier séléctionné
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            //--------------------------------
            try { Process.Start("explorer", this.DossierCapture); }
            catch { };
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Capture de zone
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //--------------------------------
            CaptureZone(true);
            //--------------------------------
        }
        // Capture  complète
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //--------------------------------
            CaptureAlls(true);
            //--------------------------------
        }
        // Capture  complète
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //--------------------------------
            CaptureWin(true);
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Change le dossier
        private void changerLeDossierDesCapturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //--------------------------------
            FolderBrowserDialog diag = new FolderBrowserDialog();
            diag.Description = MenuLang.Dialog_ChooseFolder + "...";
            if (diag.ShowDialog() == DialogResult.OK)
            {
                this.toolStripStatusLabel1.Text = diag.SelectedPath;
                this.DossierCapture = diag.SelectedPath;
                this.config.AppSettings.Settings["SavePathFolder"].Value = diag.SelectedPath;
                LoadCaptureExplorer();
            }
            //--------------------------------
        }
        // Notifs active/desactive
        private void notificationsLorsDesCapturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //--------------------------------
            notificationsLorsDesCapturesToolStripMenuItem.Checked = !this.notificationsLorsDesCapturesToolStripMenuItem.Checked;
            this.config.AppSettings.Settings["Notify"].Value = notificationsLorsDesCapturesToolStripMenuItem.Checked.ToString();
            //--------------------------------
        }
        // Autostart active/desactive
        private void éxecuterAuDémarrageDeWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //--------------------------------
            éxecuterAuDémarrageDeWindowsToolStripMenuItem.Checked = !this.éxecuterAuDémarrageDeWindowsToolStripMenuItem.Checked;
            this.config.AppSettings.Settings["StartOnBoot"].Value = éxecuterAuDémarrageDeWindowsToolStripMenuItem.Checked.ToString();

            // Startup + Captures + .bat
            string shortcutname = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".bat";
            if (this.éxecuterAuDémarrageDeWindowsToolStripMenuItem.Checked)
            {
                try { File.AppendAllText(shortcutname, @"start """" """ + Application.ExecutablePath + @""" --hide"); }
                catch { }
            }
            else
            {
                try { File.Delete(shortcutname); }
                catch { }
            }
            //--------------------------------
        }

        // Change de langue
        private void changeLanguageClick(object sender, EventArgs e)
        {
            //--------------------------------
            this.config.AppSettings.Settings["Language"].Value = (string)((ToolStripMenuItem)sender).Tag;
            Application.Restart();
            //--------------------------------
        }

        // Ouvre l'a propos
        private void àProposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //--------------------------------
            AfficherAPropos();
            //--------------------------------
        }
        // Quitte
        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //--------------------------------
            Application.Exit();
            //--------------------------------
        }

        // Mise a jouor
        private void miseÀJourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //--------------------------------
            checkUpdate(false);
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Désactive le chrono et cache la fenetre
        private void CaptureStart(bool pFromGUI)
        {
            //--------------------------------
            this.timer1.Enabled = false;
            if (pFromGUI)
            {
                this.Hide();
                Thread.Sleep(300); // Attend que la fenetre se cache avant de capturer
            }
            //--------------------------------
        }
        // Active le chrono et affiche la fenetre
        private void CaptureStop(bool pFromGUI)
        {
            //--------------------------------
            this.timer1.Enabled = true;
            if (pFromGUI) this.Show();
            //--------------------------------
        }


        // Capture  complète
        private void CaptureAlls(bool pFromGUI)
        {
            //--------------------------------
            CaptureStart(pFromGUI);

            // Récupère tous les écrans
            int screenx = SystemInformation.VirtualScreen.Left;
            int screeny = SystemInformation.VirtualScreen.Top;
            int screenw = SystemInformation.VirtualScreen.Width;
            int screenh = SystemInformation.VirtualScreen.Height;

            Bitmap desktop = new Bitmap(screenw, screenh, PixelFormat.Format32bppArgb);
            Graphics.FromImage(desktop).CopyFromScreen(screenx, screeny, 0, 0, new Size(screenw, screenh));
            CaptureSaveBitmap(desktop);

            CaptureStop(pFromGUI);
            //--------------------------------
        }
        // Capture une zone
        private void CaptureZone(bool pFromGUI)
        {
            //--------------------------------
            CaptureStart(pFromGUI);

            // Récupère tous les écrans
            int screenx = SystemInformation.VirtualScreen.Left;
            int screeny = SystemInformation.VirtualScreen.Top;
            int screenw = SystemInformation.VirtualScreen.Width;
            int screenh = SystemInformation.VirtualScreen.Height;

            // Créer la zone de séléction
            Panel pan = new Panel();
            pan.BorderStyle = BorderStyle.None;
            pan.BackColor = Color.Green;
            pan.Size = Size.Empty;
            pan.Dock = DockStyle.Fill;

            // Crée le panel de séléction de zone (le contour)
            Panel zone = new Panel();
            zone.BorderStyle = BorderStyle.None;
            zone.BackColor = Color.Red;
            zone.Size = Size.Empty;
            zone.Padding = new Padding(TAILLE_RED_BORDER);
            zone.Controls.Add(pan);

            // Label d'infos
            Label label = new Label();
            label.Text = MenuLang.Area_Label;
            label.Font = new Font("Segoe UI", 26F, FontStyle.Regular);
            label.AutoSize = false;
            label.ForeColor = Color.White;
            label.Size = new Size(1000, 100);
            label.Location = new Point(50, 50);

            // Crée la form de fond pour la capture
            Form back = new Form();
            back.FormBorderStyle = FormBorderStyle.None;
            back.ShowInTaskbar = false;
            back.Cursor = Cursors.Cross;
            back.BackColor = Color.Black;
            back.TransparencyKey = Color.Green;
            back.Opacity = .75;
            back.TopMost = true;
            back.Show(); // Afficher avant de déplacer
            back.Location = new Point(screenx, screeny);
            back.Size = new Size(screenw, screenh);
            back.Controls.Add(zone);
            back.Controls.Add(label);


            // Gère la séléction de zone
            back.MouseDown += new MouseEventHandler((a, b) => // Démarre la séléction
            {
                this.DrawingCourse = true;
                this.DrawingStartPoint = b.Location;
            });
            back.MouseUp += new MouseEventHandler((a, b) => // Arrete la séléction
            {
                this.DrawingCourse = false;
                if (zone.Width > 0 && zone.Height > 0) // Si aucune selection
                {
                    back.Dispose();

                    // Sauvegarde l'image de la zone
                    Bitmap desktop = new Bitmap(screenw, screenh, PixelFormat.Format32bppArgb);
                    Graphics.FromImage(desktop).CopyFromScreen(screenx, screeny, 0, 0, new Size(screenw, screenh));
                    Bitmap save = new Bitmap(zone.Width - TAILLE_RED_BORDER * 2, zone.Height - TAILLE_RED_BORDER * 2, PixelFormat.Format32bppArgb); 
                    Graphics.FromImage(save).DrawImage(desktop, -(zone.Location.X + TAILLE_RED_BORDER), -(zone.Location.Y + TAILLE_RED_BORDER));
                    CaptureSaveBitmap(save);

                    CaptureStop(pFromGUI);
                }
            });
            back.MouseMove += new MouseEventHandler((a, b) =>
            {
                if (this.DrawingCourse)
                {
                    // Déplace le panel
                    int backx = Math.Min(this.DrawingStartPoint.X, b.X) - TAILLE_RED_BORDER;
                    int backy = Math.Min(this.DrawingStartPoint.Y, b.Y) - TAILLE_RED_BORDER;
                    int backw = Math.Max(this.DrawingStartPoint.X, b.X) - Math.Min(this.DrawingStartPoint.X, b.X) + TAILLE_RED_BORDER * 2;
                    int backh = Math.Max(this.DrawingStartPoint.Y, b.Y) - Math.Min(this.DrawingStartPoint.Y, b.Y) + TAILLE_RED_BORDER * 2;
                    zone.Location = new Point(backx, backy);
                    zone.Size = new Size(backw, backh);
                }
            });
            //--------------------------------
        }
        // Capture de fenetre
        private void CaptureWin(bool pFromGUI)
        {
            //--------------------------------
            CaptureStart(pFromGUI);

            // Récupère la zone de la fenetre
            Rectangle rect = new Rectangle();
            IntPtr hwd = GetForegroundWindow();
            /* Sous Windows 10 on utilise une librairie unique à Windows 10 (dwmapi avec DwmGetWindowAttribute) pour pouvoir
             * récupérer les bordures fines de Windows 10 mais sous Windows 7 il faut utiliser la librairie classique (user32
             * avec GetWindowRect) pour récupérer les grosses bordures. */
            if (DwmGetWindowAttribute(hwd, 9, ref rect, Marshal.SizeOf(typeof(Rectangle))) != IntPtr.Zero && !GetWindowRect(hwd, ref rect)) this.notifyIcon1.ShowBalloonTip(5000, "Captures", MenuLang.Error_Lib, ToolTipIcon.Error);
            else
            {
                // Sauvegarde
                Bitmap desktop = new Bitmap(rect.Width - rect.X, rect.Height - rect.Y, PixelFormat.Format32bppArgb);
                Graphics.FromImage(desktop).CopyFromScreen(new Point(rect.X, rect.Y), Point.Empty, desktop.Size);
                CaptureSaveBitmap(desktop);

                Thread.Sleep(500);
            }

            CaptureStop(pFromGUI);
            //--------------------------------
        }


        // Sauvegarde une capture
        private void CaptureSaveBitmap(Bitmap pImage)
        {
            //--------------------------------
            if (!Directory.Exists(this.toolStripStatusLabel1.Text))
            {
                try { Directory.CreateDirectory(this.toolStripStatusLabel1.Text); }
                catch
                {
                    this.notifyIcon1.ShowBalloonTip(5000, "Captures", MenuLang.Error_AccessFolder, ToolTipIcon.Error);
                    return;
                }
            }

            ImageFormat format;
            string formattext = this.SelectedFormat.Text; // Recupère le format

            switch (formattext)
            {
                case ".PNG":
                    format = ImageFormat.Png;
                    break;

                case ".JPEG":
                    format = ImageFormat.Jpeg;
                    break;

                case ".JPG":
                    format = ImageFormat.Jpeg;
                    break;

                case ".BMP":
                    format = ImageFormat.Bmp;
                    break;

                case ".ICO":
                    format = ImageFormat.Icon;
                    break;

                case ".GIF":
                    format = ImageFormat.Gif;
                    break;

                default:
                    format = ImageFormat.Png;
                    break;
            }

            string file = // Créer le nom du fichier
                this.DossierCapture +
                @"\" +
                DateTime.Now.Year +
                DateTime.Now.Month +
                DateTime.Now.Day +
                "_" +
                DateTime.Now.Hour +
                DateTime.Now.Minute +
                DateTime.Now.Second +
                "_" +
                DateTime.Now.Millisecond +
                formattext.ToLower();

            try // Sauvegarde
            {
                pImage.Save(file, format);
                if (this.notificationsLorsDesCapturesToolStripMenuItem.Checked) this.notifyIcon1.ShowBalloonTip(5000, "Captures", MenuLang.Capture_Sucess, ToolTipIcon.Info);
                LoadCaptureExplorer(file);
            }
            catch
            {
                if (this.notificationsLorsDesCapturesToolStripMenuItem.Checked) this.notifyIcon1.ShowBalloonTip(5000, "Captures", MenuLang.Captures_Fail, ToolTipIcon.Error);
            }
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        // Gestion des touches F1, F2 etc..
        private void timer1_Tick(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // 112 = F1 donc 111 + 1 (sans le F)
            if (IsPressed(this.SelectedKeyAlls)) CaptureAlls(false);
            else if (IsPressed(this.SelectedKeyWin)) CaptureWin(false);
            else if (IsPressed(this.SelectedKeyZone)) CaptureZone(false);
            //-----------------------------------------------
        }
        // Vérifi l'appui sur une touche
        private bool IsPressed(ToolStripItem pMenu)
        {
            //-----------------------------------------------
            /* -32767 = Clé presse en temps réel.
             * 1 = Si la clé à été préssée depuis la dernière vérif.
             * 0 = Clé non pressée.
             */
            if (GetAsyncKeyState(111 + Convert.ToInt32(pMenu.Text.Replace("F", ""))) == 0) return false;
            else return true;
            //-----------------------------------------------
        }
        //=================================================================



        //=================================================================
        // Affiche la fenetre de capture
        private void AfficherCaptures()
        {
            //--------------------------------
            //--------------------------------
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Focus();
            //--------------------------------
        }
        // Affiche la fenetre des infos
        private void AfficherAPropos()
        {
            //--------------------------------
            About form = new About();
            form.Owner = this;
            form.ShowDialog();
            form.Dispose();
            //--------------------------------
        }
        //=================================================================



        //=================================================================
        private void checkUpdate(bool silent)
        {
            //--------------------------------
            try
            {
                string remoteUri = "https://raw.githubusercontent.com/TheRake66/Captures/master/version";

                string lastversion;
                string currentversion = typeof(Menu).Assembly.GetName().Version.ToString();

                using (WebClient client = new WebClient())
                {
                    lastversion = Encoding.UTF8.GetString(client.DownloadData(remoteUri));
                }

                if (lastversion.Equals(currentversion))
                {
                    if (!silent) MessageBox.Show(MenuLang.Update_None, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (MessageBox.Show(MenuLang.Update_Valaible + 
                        Environment.NewLine +
                        currentversion + " → " + lastversion
                        , "Captures", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        try { Process.Start("explorer", "https://github.com/TheRake66/Captures"); }
                        catch { }
                    }
                }
            }
            catch
            {
                if (!silent) MessageBox.Show(MenuLang.Update_Fail, "Captures", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //--------------------------------
        }
        //=================================================================
    }
}
